using System;
using System.Collections.Generic;
using AppKit;
using Foundation;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class BindingObjectSelectorControl : NSView
	{
		internal class ObjectOutlineView : NSOutlineView
		{
			private IReadOnlyList<ObjectTreeElement> viewModel;
			public IReadOnlyList<ObjectTreeElement> ViewModel {
				get => this.viewModel;
				set {
					if (this.viewModel != value) {
						this.viewModel = value;
						var dataSource = new ObjectOutlineViewDataSource (this.viewModel);
						Delegate = new ObjectOutlineViewDelegate (dataSource);
						DataSource = dataSource;
					}

					if (this.viewModel != null) {
						ReloadData ();

						ExpandItem (null, true);
					}
				}
			}

			public ObjectOutlineView ()
			{
				Initialize ();
			}

			// Called when created from unmanaged code
			public ObjectOutlineView (IntPtr handle) : base (handle)
			{
				Initialize ();
			}

			// Called when created directly from a XIB file
			[Export ("initWithCoder:")]
			public ObjectOutlineView (NSCoder coder) : base (coder)
			{
				Initialize ();
			}

			[Export ("validateProposedFirstResponder:forEvent:")]
			public bool ValidateProposedFirstResponder (NSResponder responder, NSEvent forEvent)
			{
				return true;
			}

			public void Initialize ()
			{
				AutoresizingMask = NSViewResizingMask.WidthSizable;
				HeaderView = null;
				TranslatesAutoresizingMaskIntoConstraints = false;
			}
		}

		internal class ObjectOutlineViewDelegate : NSOutlineViewDelegate
		{
			private ObjectOutlineViewDataSource dataSource;

			public ObjectOutlineViewDelegate (ObjectOutlineViewDataSource dataSource)
			{
				this.dataSource = dataSource;
			}

			public override nfloat GetRowHeight (NSOutlineView outlineView, NSObject item)
			{
				return PropertyEditorControl.DefaultControlHeight;
			}

			public override NSView GetView (NSOutlineView outlineView, NSTableColumn tableColumn, NSObject item)
			{
				var labelContainer = (UnfocusableTextField)outlineView.MakeView ("type", this);
				if (labelContainer == null) {
					labelContainer = new UnfocusableTextField {
						Identifier = "type",
					};
				}
				var target = (item as NSObjectFacade).Target;

				switch (target) {
				case KeyValuePair<string, SimpleCollectionView> kvp:
					labelContainer.StringValue = kvp.Key;
					break;
				case TypeInfo info:
					labelContainer.StringValue = info.Name;
					break;
				default:
					labelContainer.StringValue = "Type Not Supported";
					break;
				}

				return labelContainer;
			}

			public override bool ShouldSelectItem (NSOutlineView outlineView, NSObject item)
			{
				var target = (item as NSObjectFacade).Target;
				switch (target) {
				case KeyValuePair<string, SimpleCollectionView> kvp:
					return false;
				case TypeInfo info:
					return true;

				default:
					return false;
				}
			}
		}

		internal class ObjectOutlineViewDataSource : NSOutlineViewDataSource
		{
			public IReadOnlyList<ObjectTreeElement> ViewModel { get; }

			internal ObjectOutlineViewDataSource (IReadOnlyList<ObjectTreeElement> viewModel)
			{
				if (viewModel == null)
					throw new ArgumentNullException (nameof (viewModel));

				ViewModel = viewModel;
			}

			public override nint GetChildrenCount (NSOutlineView outlineView, NSObject item)
			{
				var childCount = 0;
				if (item == null) {
					childCount = this.ViewModel != null ? this.ViewModel.Count () : 0;
				} else {
					var target = (item as NSObjectFacade).Target;
					switch (target) {
					case KeyValuePair<string, SimpleCollectionView> kvp:
						childCount = kvp.Value.Count;
						break;
					case TypeInfo info:
						childCount = 0;
						break;
					default:
						childCount = 0;
						break;
					}
				}

				return childCount;
			}

			public override NSObject GetChild (NSOutlineView outlineView, nint childIndex, NSObject item)
			{
				object element;

				if (item == null) {
					element = this.ViewModel.ElementAt ((int)childIndex);
				} else {
					var target = (item as NSObjectFacade).Target;
					switch (target) {
					case KeyValuePair<string, SimpleCollectionView> kvp:
						element = kvp.Value[(int)childIndex];
						break;
					case TypeInfo info:
						element = info;
						break;
					default:
						return null;
					}
				}

				return new NSObjectFacade (element);
			}

			public override bool ItemExpandable (NSOutlineView outlineView, NSObject item)
			{
				var target = (item as NSObjectFacade).Target;
				switch (target) {
				case KeyValuePair<string, SimpleCollectionView> kvp:
					return kvp.Value.Count > 0;
				case TypeInfo info:
					return false;
				default:
					return false;
				}
			}
		}

		internal ObjectOutlineView objectOutlineView;

		internal BindingObjectSelectorControl (CreateBindingViewModel viewModel)
		{
			if (viewModel == null)
				throw new ArgumentNullException (nameof (viewModel));

			this.objectOutlineView = new ObjectOutlineView ();
			TranslatesAutoresizingMaskIntoConstraints = false;

			viewModel.PropertyChanged += (sender, e) => {
				if (e.PropertyName == nameof (CreateBindingViewModel.ShowObjectSelector)) {
					Hidden = !viewModel.ShowObjectSelector;

					if (viewModel.ShowObjectSelector && viewModel.ObjectElementRoots != null) {
						this.objectOutlineView.ViewModel = viewModel.ObjectElementRoots.Value;
					};
				}
			};
		}
	}
}
