using System;
using System.Collections.Generic;
using AppKit;
using Foundation;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class StatefulPropertySelectorControl
		: NotifyingView<StatePropertyGroupViewModel>
	{
		private readonly NSOutlineView outlineView;

		public StatefulPropertySelectorControl (IHostResourceProvider hostResources)
		{
			if (hostResources == null)
				throw new ArgumentNullException (nameof (hostResources));

			var scroll = new NSScrollView {
				TranslatesAutoresizingMaskIntoConstraints = false
			};

			var datasource = new StatefulPropertySelectorDataSource (ViewModel);

			var statefulPropertySelectorDelegate = new StatefulPropertySelectorDelegate (hostResources, datasource);
			this.outlineView = new NSOutlineView {
				Delegate = statefulPropertySelectorDelegate,
				AutoresizingMask = NSViewResizingMask.WidthSizable,
				HeaderView = null,
				Action = new ObjCRuntime.Selector ("onActivatedItem"),
				Target = this
			};

			var col = new NSTableColumn ();
			this.outlineView.AddColumn (col);
			this.outlineView.OutlineTableColumn = col;
			scroll.DocumentView = this.outlineView;

			AddSubview (scroll);
		}
	}

	internal class StatefulPropertySelectorDataSource
			: NSOutlineViewDataSource
	{
		public StatefulPropertySelectorDataSource (StatePropertyGroupViewModel viewModel)
		{
			this.viewModel = viewModel;
		}

		public override nint GetChildrenCount (NSOutlineView outlineView, NSObject item)
		{
			if (item == null) {
				return this.viewModel.Properties?.Count ?? 0;
			} else if (((NSObjectFacade)item).Target is KeyValuePair<string, SimpleCollectionView> kvp) {
				return kvp.Value.Count;
			}

			return base.GetChildrenCount (outlineView, item);
		}

		public override NSObject GetChild (NSOutlineView outlineView, nint childIndex, NSObject item)
		{
			if (item == null) {
				return new NSObjectFacade (this.viewModel.Properties[(int)childIndex]);
			} else if (((NSObjectFacade)item).Target is KeyValuePair<string, SimpleCollectionView> kvp) {
				return new NSObjectFacade (kvp.Value[(int)childIndex]);
			}

			return base.GetChild (outlineView, childIndex, item);
		}

		public override bool ItemExpandable (NSOutlineView outlineView, NSObject item)
		{
			return !(((NSObjectFacade)item).Target is ITypeInfo);
		}

		private StatePropertyGroupViewModel viewModel;
	}

	internal class StatefulPropertySelectorDelegate
		: NSOutlineViewDelegate
	{
		private readonly PropertyEditorSelector editorSelector = new PropertyEditorSelector ();
		private IHostResourceProvider hostResources;
		private StatefulPropertySelectorDataSource dataSource;

		public StatefulPropertySelectorDelegate (IHostResourceProvider hostResources, StatefulPropertySelectorDataSource dataSource)
		{
			if (hostResources == null)
				throw new ArgumentNullException (nameof (hostResources));
			if (dataSource == null)
				throw new ArgumentNullException (nameof (dataSource));

			this.hostResources = hostResources;
			this.dataSource = dataSource;
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
			}

			return null; // TODO 
		}

		public override NSView GetView (NSOutlineView outlineView, NSTableColumn tableColumn, NSObject item)
		{
			var label = (UnfocusableTextField)outlineView.MakeView (LabelId, outlineView);
			if (label == null) {
				label = new UnfocusableTextField {
					Identifier = LabelId
				};
			}

			string text = String.Empty;
			var facade = (NSObjectFacade)item;
			if (facade.Target is KeyValuePair<string, SimpleCollectionView> kvp)
				text = kvp.Key;
			else if (facade.Target is ITypeInfo type)
				text = type.Name;

			label.StringValue = text;
			return label;
		}

		private const string LabelId = "label";
	}
}
