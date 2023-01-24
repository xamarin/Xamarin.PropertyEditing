using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AppKit;
using Foundation;
using ObjCRuntime;

using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class PropertyTableDelegate
		: NSOutlineViewDelegate
	{
		public PropertyTableDelegate (IHostResourceProvider hostResources, PropertyTableDataSource dataSource)
		{
			if (hostResources == null)
				throw new ArgumentNullException (nameof (hostResources));
			if (dataSource == null)
				throw new ArgumentNullException (nameof (dataSource));

			this.hostResources = hostResources;
			this.dataSource = dataSource;
		}

		public void UpdateExpansions (NSOutlineView outlineView)
		{
			this.isUpdatingExpansions = true;

			if (!String.IsNullOrWhiteSpace (this.dataSource.DataContext.FilterText)) {
				outlineView.ExpandItem (null, true);
			} else {
				foreach (PanelGroupViewModel g in this.dataSource.DataContext.ArrangedEditors) {
					NSObjectFacade item;
					if (!this.dataSource.TryGetFacade (g, out item))
						continue;

					if (this.dataSource.DataContext.GetIsExpanded (g.Category))
						EnsureOpenOrClose (outlineView, item, open: true);
					else
						EnsureOpenOrClose (outlineView, item, open: false);
				}
			}
			this.isUpdatingExpansions = false;
		}

		public override NSView GetView (NSOutlineView outlineView, NSTableColumn tableColumn, NSObject item)
		{
			GetVMGroupCellItendifiterFromFacade (item, out EditorViewModel evm, out PanelGroupViewModel group, out var cellIdentifier);

			if (group != null) {
				var categoryContainer = (CategoryContainerControl)outlineView.MakeView (CategoryIdentifier, this);
				if (categoryContainer == null) {
					categoryContainer = new CategoryContainerControl (this.hostResources, outlineView) {
						Identifier = CategoryIdentifier,
						TableView = outlineView,
					};
				}

				((UnfocusableTextField)categoryContainer.Subviews[1]).StringValue = group.Category;

				return categoryContainer;
			}

			NSView editorOrContainer = null;
			if (this.firstCache.TryGetValue (cellIdentifier, out IEditorView editor)) {
				this.firstCache.Remove (cellIdentifier);
				editorOrContainer = (editor.NativeView is PropertyEditorControl) ? new EditorContainer (this.hostResources, editor) { Identifier = cellIdentifier } : editor.NativeView;
			} else {
				editorOrContainer = GetEditor (cellIdentifier, evm, outlineView);
				editor = ((editorOrContainer as EditorContainer)?.EditorView) ?? editorOrContainer as IEditorView;
			}

			if (editorOrContainer is EditorContainer ec) {
				ec.ViewModel = evm;
				ec.Label = evm.Name;

#if DEBUG // Currently only used to highlight which controls haven't been implemented
				if (editor == null)
					ec.LabelTextColor = NSColor.Red;
#endif
			}

			if (editor != null) {
				var ovm = evm as ObjectPropertyViewModel;

				if (!(editorOrContainer is EditorContainer)) {
					editor.ViewModel = evm;
				}

				bool openObjectRow = ovm != null && outlineView.IsItemExpanded (item);
				if (!openObjectRow) {
					var parent = outlineView.GetParent (item);
					openObjectRow = (parent != null && ((NSObjectFacade)parent).Target is ObjectPropertyViewModel);
				}

				SetRowValueBackground (editorOrContainer, openObjectRow);

				// Force a row update due to new height, but only when we are non-default
				if (editor.IsDynamicallySized) {
					nint index = outlineView.RowForItem (item);
					outlineView.NoteHeightOfRowsWithIndexesChanged (new NSIndexSet (index));
				}
			} else if (editorOrContainer is PanelHeaderEditorControl header) {
				header.ViewModel = this.dataSource.DataContext;
			}

			return editorOrContainer;
		}

		public override bool ShouldSelectItem (NSOutlineView outlineView, NSObject item)
		{
			return (!(item is NSObjectFacade) || !(((NSObjectFacade)item).Target is PanelGroupViewModel));
		}

		public override void ItemDidExpand (NSNotification notification)
		{
			if (this.isUpdatingExpansions)
				return;

			NSObjectFacade facade = notification.UserInfo.Values[0] as NSObjectFacade;
			var outline = (NSOutlineView)notification.Object;
			nint row = outline.RowForItem (facade);

			if (facade.Target is PanelGroupViewModel group) {
				this.dataSource.DataContext.SetIsExpanded (group.Category, isExpanded: true);
			}
			else if (facade.Target is ObjectPropertyViewModel ovm) {
				NSView view = outline.GetView (0, row, makeIfNecessary: false);
				SetRowValueBackground (view, valueBackground: true);
			}
		}

		public override void ItemDidCollapse (NSNotification notification)
		{
			if (this.isUpdatingExpansions)
				return;

			NSObjectFacade facade = notification.UserInfo.Values[0] as NSObjectFacade;
			var outline = (NSOutlineView)notification.Object;
			nint row = outline.RowForItem (facade);

			if (facade.Target is PanelGroupViewModel group)
				this.dataSource.DataContext.SetIsExpanded (group.Category, isExpanded: false);
			else if (facade.Target is ObjectPropertyViewModel ovm) {
				NSView view = outline.GetView (0, row, makeIfNecessary: false);
				SetRowValueBackground (view, valueBackground: false);
			}
		}

		public override void DidRemoveRowView (NSOutlineView outlineView, NSTableRowView rowView, nint row)
		{
			if (rowView.Subviews[0] is EditorContainer ec) {
				ec.ViewModel = null;
			}
		}

		public override nfloat GetRowHeight (NSOutlineView outlineView, NSObject item)
		{
			EditorViewModel vm;
			PanelGroupViewModel group;
			string cellIdentifier;
			GetVMGroupCellItendifiterFromFacade (item, out vm, out group, out cellIdentifier);

			if (group != null)
				return 24;

			if (!this.registrations.TryGetValue (cellIdentifier, out EditorRegistration registration)) {
				registration = new EditorRegistration ();

				if (cellIdentifier == nameof (PanelHeaderEditorControl)) {
					if (this.dataSource?.DataContext is PanelViewModel panelView && !panelView.IsObjectNameable) {
						registration.RowSize = 26;
					}
					else {
						registration.RowSize = 54;
					}
				} else {
					NSView editorOrContainer = GetEditor (cellIdentifier, vm, outlineView);
					IEditorView view = ((editorOrContainer as EditorContainer)?.EditorView) ?? editorOrContainer as IEditorView;

					if (view == null) {
						registration.RowSize = 24;
					} else if (view.IsDynamicallySized) {
						registration.SizingInstance = view;
					} else {
						this.registrations[cellIdentifier] = registration = new EditorRegistration {
							RowSize = view.GetHeight (vm)
						};

						this.firstCache[cellIdentifier] = view;
					}
				}

				this.registrations[cellIdentifier] = registration;
			}

			// The double cast below is needed for now, while the return value is type ObjCRuntime.nfloat
			// in order for the integral to floating point conversion to work properly. Later when ObjCRuntime.nfloat
			// is replaced with System.Runtime.InteropServices.NFloat that won't be needed (but can't hurt).
			nfloat rowHeight = (nfloat)(double)registration.GetHeight (vm);
			return rowHeight;
		}

		private class EditorRegistration
		{
			public nint RowSize;
			public IEditorView SizingInstance;

			public nint GetHeight (EditorViewModel vm)
			{
				if (SizingInstance != null)
					return SizingInstance.GetHeight (vm);
				else
					return RowSize;
			}
		}

		public const string CategoryIdentifier = "label";

		private PropertyTableDataSource dataSource;
		private bool isUpdatingExpansions;
		private readonly PropertyEditorSelector editorSelector = new PropertyEditorSelector ();

		private readonly IHostResourceProvider hostResources;
		private readonly Dictionary<string, EditorRegistration> registrations = new Dictionary<string, EditorRegistration> ();
		private readonly Dictionary<string, IEditorView> firstCache = new Dictionary<string, IEditorView> ();

		private void EnsureOpenOrClose (NSOutlineView outline, NSObjectFacade item, bool open)
		{
			if (open)
				outline.ExpandItem (item);
			else
				outline.CollapseItem (item);

			NSView view = GetViewForItem (outline, item, makeIfNeccessary: true);
			if (view.Subviews[0] is NSButton expander)
				expander.State = (open) ? NSCellStateValue.On : NSCellStateValue.Off;
		}

		private NSView GetViewForItem (NSOutlineView outline, NSObjectFacade facade, bool makeIfNeccessary = false)
		{
			nint row = outline.RowForItem (facade);
			return outline.GetView (0, row, makeIfNeccessary);
		}

		private void SetRowValueBackground (NSView view, bool valueBackground)
		{
			if (view == null)
				return;

			if (valueBackground) {
				var c = this.hostResources.GetNamedColor (NamedResources.ValueBlockBackgroundColor);
				view.SetValueForKey (c, new NSString ("backgroundColor"));
			} else {
				view.SetValueForKey (NSColor.Clear, new NSString ("backgroundColor"));
			}
		}

		private NSView GetEditor (string identifier, EditorViewModel vm, NSOutlineView outlineView)
		{
			var view = outlineView.MakeView (identifier, this);
			if (view != null)
				return view;

			if (vm != null) {
				IEditorView editor = this.editorSelector.GetEditor (this.hostResources, vm);

				var editorControl = editor?.NativeView as PropertyEditorControl;
				if (editorControl != null) {
					editorControl.TableView = outlineView;
				} else if (editor?.NativeView != null) {
					editor.NativeView.Identifier = identifier;
					return editor.NativeView;
				}

				return new EditorContainer (this.hostResources, editor) { Identifier = identifier };
			} else
				return new PanelHeaderEditorControl (this.hostResources);
		}

		private void GetVMGroupCellItendifiterFromFacade (NSObject item, out EditorViewModel vm, out PanelGroupViewModel group, out string cellIdentifier)
		{
			var facade = (NSObjectFacade)item;
			vm = facade.Target as EditorViewModel;
			group = facade.Target as PanelGroupViewModel;
			cellIdentifier = facade.Target == null
								   ? nameof (PanelHeaderEditorControl)
								   : (group == null) ? vm.GetType ().FullName : group.Category;
		}
	}
}
