using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AppKit;
using Foundation;

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
			this.isExpanding = true;

			if (!String.IsNullOrWhiteSpace (this.dataSource.DataContext.FilterText)) {
				outlineView.ExpandItem (null, true);
			} else {
				foreach (PanelGroupViewModel g in this.dataSource.DataContext.ArrangedEditors) {
					NSObject item;
					if (!this.dataSource.TryGetFacade (g, out item))
						continue;

					if (this.dataSource.DataContext.GetIsExpanded (g.Category))
						outlineView.ExpandItem (item);
					else
						outlineView.CollapseItem (item);
				}
			}
			this.isExpanding = false;
		}

		public override NSView GetView (NSOutlineView outlineView, NSTableColumn tableColumn, NSObject item)
		{
			EditorViewModel evm;
			PropertyViewModel vm;
			PanelGroupViewModel group;
			string cellIdentifier;
			GetVMGroupCellItendifiterFromFacade (item, out evm, out group, out cellIdentifier);

			if (group != null) {
				var labelContainer = (UnfocusableTextField)outlineView.MakeView (LabelIdentifier, this);
				if (labelContainer == null) {
					labelContainer = new UnfocusableTextField {
						Identifier = LabelIdentifier,
					};
				}

				labelContainer.StringValue = group.Category;

				if (this.dataSource.DataContext.GetIsExpanded (group.Category)) {
					SynchronizationContext.Current.Post (s => {
						outlineView.ExpandItem (item);
					}, null);
				}

				return labelContainer;
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
				ec.Label = evm.Name + ":";

#if DEBUG // Currently only used to highlight which controls haven't been implemented
				if (editor == null)
					ec.LabelTextColor = NSColor.Red;
#endif
			}

			if (editor != null) {
				nint index = outlineView.RowForItem (item);
				if (editor.NativeView is PropertyEditorControl pec) {
					pec.TableRow = index;
				}

				editor.ViewModel = evm;

				// Force a row update due to new height, but only when we are non-default
				if (editor.IsDynamicallySized) {
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
			if (this.isExpanding)
				return;

			NSObjectFacade facade = notification.UserInfo.Values[0] as NSObjectFacade;
			var group = facade.Target as PanelGroupViewModel;
			if (group != null)
				this.dataSource.DataContext.SetIsExpanded (group.Category, isExpanded: true);
		}

		public override void ItemDidCollapse (NSNotification notification)
		{
			if (this.isExpanding)
				return;

			NSObjectFacade facade = notification.UserInfo.Values[0] as NSObjectFacade;
			var group = facade.Target as PanelGroupViewModel;
			if (group != null)
				this.dataSource.DataContext.SetIsExpanded (group.Category, isExpanded: false);
		}

		public override nfloat GetRowHeight (NSOutlineView outlineView, NSObject item)
		{
			EditorViewModel vm;
			PanelGroupViewModel group;
			string cellIdentifier;
			GetVMGroupCellItendifiterFromFacade (item, out vm, out group, out cellIdentifier);

			if (group != null)
				return 20;

			if (!this.registrations.TryGetValue (cellIdentifier, out EditorRegistration registration)) {
				registration = new EditorRegistration ();

				if (cellIdentifier == nameof(PanelHeaderEditorControl)) {
					registration.RowSize = 44;
				} else {
					NSView editorOrContainer = GetEditor (cellIdentifier, vm, outlineView);
					IEditorView view = ((editorOrContainer as EditorContainer)?.EditorView) ?? editorOrContainer as IEditorView;

					if (view == null) {
						registration.RowSize = PropertyEditorControl.DefaultControlHeight;
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

			return registration.GetHeight (vm);
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

		public const string LabelIdentifier = "label";

		private PropertyTableDataSource dataSource;
		private bool isExpanding;
		private readonly PropertyEditorSelector editorSelector = new PropertyEditorSelector ();

		private readonly IHostResourceProvider hostResources;
		private readonly Dictionary<string, EditorRegistration> registrations = new Dictionary<string, EditorRegistration> ();
		private readonly Dictionary<string, IEditorView> firstCache = new Dictionary<string, IEditorView> ();

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
