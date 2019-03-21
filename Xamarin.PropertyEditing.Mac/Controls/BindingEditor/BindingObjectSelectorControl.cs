using System;
using System.Collections.Generic;
using AppKit;
using Foundation;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class BindingObjectSelectorControl : NSView
	{
		internal class ObjectOutlineView : BaseSelectorOutlineView
		{
			private IReadOnlyList<ObjectTreeElement> itemsSource;
			public IReadOnlyList<ObjectTreeElement> ItemsSource {
				get => this.itemsSource;
				set {
					if (this.itemsSource != value) {
						this.itemsSource = value;

						DataSource = new ObjectOutlineViewDataSource (this.itemsSource); ;
						Delegate = new ObjectOutlineViewDelegate ();
					}

					ReloadData ();

					ExpandItem (null, true);
				}
			}
		}

		internal class ObjectOutlineViewDelegate : BaseOutlineViewDelegate
		{
			private const string TypeIdentifier = "type";

			public override NSView GetView (NSOutlineView outlineView, NSTableColumn tableColumn, NSObject item)
			{
				var labelContainer = (UnfocusableTextField)outlineView.MakeView (TypeIdentifier, this);
				if (labelContainer == null) {
					labelContainer = new UnfocusableTextField {
						Identifier = TypeIdentifier,
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
					labelContainer.StringValue = Properties.Resources.TypeNotSupported;
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
			public IReadOnlyList<ObjectTreeElement> ItemsSource { get; }

			internal ObjectOutlineViewDataSource (IReadOnlyList<ObjectTreeElement> itemsSource)
			{
				if (itemsSource == null)
					throw new ArgumentNullException (nameof (itemsSource));

				ItemsSource = itemsSource;
			}

			public override nint GetChildrenCount (NSOutlineView outlineView, NSObject item)
			{
				if (item == null) {
					return ItemsSource != null ? ItemsSource.Count : 0;
				} else {
					var target = (item as NSObjectFacade).Target;
					switch (target) {
					case KeyValuePair<string, SimpleCollectionView> kvp:
						return kvp.Value.Count;
					case TypeInfo info:
						return 0;
					default:
						return 0;
					}
				}
			}

			public override NSObject GetChild (NSOutlineView outlineView, nint childIndex, NSObject item)
			{
				object element;

				if (item == null) {
					element = ItemsSource.ElementAt ((int)childIndex);
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

		private ObjectOutlineView objectOutlineView;

		private const string ObjectSelectorColId = "ObjectSelectorColumn";

		private readonly CreateBindingViewModel viewModel;

		internal BindingObjectSelectorControl (CreateBindingViewModel viewModel)
		{
			if (viewModel == null)
				throw new ArgumentNullException (nameof (viewModel));

			this.viewModel = viewModel;

			this.objectOutlineView = new ObjectOutlineView ();
			TranslatesAutoresizingMaskIntoConstraints = false;

			this.objectOutlineView.Activated += OnObjectOutlineViewSelected;

			var resourceColumn = new NSTableColumn (ObjectSelectorColId);
			this.objectOutlineView.AddColumn (resourceColumn);

			// Set OutlineTableColumn or the arrows showing children/expansion will not be drawn
			this.objectOutlineView.OutlineTableColumn = resourceColumn;

			// create a table view and a scroll view
			var outlineViewContainer = new NSScrollView {
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			// add the panel to the window
			outlineViewContainer.DocumentView = this.objectOutlineView;
			AddSubview (outlineViewContainer);

			AddConstraints (new[] {
				NSLayoutConstraint.Create (outlineViewContainer, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this, NSLayoutAttribute.Top, 1f, 35f),
				NSLayoutConstraint.Create (outlineViewContainer, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this, NSLayoutAttribute.Left, 1f, 5f),
				NSLayoutConstraint.Create (outlineViewContainer, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this, NSLayoutAttribute.Width, 1f, -10f),
				NSLayoutConstraint.Create (outlineViewContainer, NSLayoutAttribute.Height, NSLayoutRelation.Equal,this, NSLayoutAttribute.Height, 1f, -40f),
			});

			viewModel.PropertyChanged += OnPropertyChanged;
		}

		void OnPropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof (CreateBindingViewModel.ShowObjectSelector)) {
				Hidden = !this.viewModel.ShowObjectSelector;

				if (this.viewModel.ShowObjectSelector && this.viewModel.ObjectElementRoots != null) {
					this.objectOutlineView.ItemsSource = this.viewModel.ObjectElementRoots.Value;
				};
			}
		}


		private void OnObjectOutlineViewSelected (object sender, EventArgs e)
		{
			if (sender is ObjectOutlineView rov) {
				if (rov.SelectedRow != -1) {
					if (rov.ItemAtRow (rov.SelectedRow) is NSObjectFacade item) {
						if (item.Target is Resource resource) {
							this.viewModel.SelectedResource = resource;
						}
					}
				}
			}
		}

	}
}
