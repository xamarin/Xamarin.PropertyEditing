using System;
using System.Collections.Generic;
using System.Linq;
using AppKit;
using Foundation;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class BindingResourceSelectorControl : NSView
	{
		internal class ResourceOutlineView : BaseSelectorOutlineView
		{
			private ILookup<ResourceSource, Resource> itemsSource;
			public ILookup<ResourceSource, Resource> ItemsSource
			{
				get => this.itemsSource;
				set
				{
					if (this.itemsSource != value) {
						this.itemsSource = value;

						DataSource = new ResourceOutlineViewDataSource (this.itemsSource);
						Delegate = new ResourceOutlineViewDelegate ();
					}

					ReloadData ();

					ExpandItem (null, true);
				}
			}
		}

		internal class ResourceOutlineViewDelegate : BaseOutlineViewDelegate
		{

			private const string ResourceIdentifier = "resource";

			public override NSView GetView (NSOutlineView outlineView, NSTableColumn tableColumn, NSObject item)
			{
				var labelContainer = (UnfocusableTextField)outlineView.MakeView (ResourceIdentifier, this);
				if (labelContainer == null) {
					labelContainer = new UnfocusableTextField {
						Identifier = ResourceIdentifier,
					};
				}
				var target = (item as NSObjectFacade).Target;

				switch (target) {
				case IGrouping<ResourceSource, Resource> kvp:
					labelContainer.StringValue = kvp.Key.Name;
					break;
				case Resource resource:
					labelContainer.StringValue = resource.Name;
					break;
				default:
					labelContainer.StringValue = Properties.Resources.ResourceNotSupported;
					break;
				}

				return labelContainer;
			}

			public override bool ShouldSelectItem (NSOutlineView outlineView, NSObject item)
			{
				var target = (item as NSObjectFacade).Target;
				switch (target) {
				case IGrouping<ResourceSource, Resource> kvp:
					return false;
				case Resource resource:
					return true;

				default:
					return false;
				}
			}
		}

		internal class ResourceOutlineViewDataSource : NSOutlineViewDataSource
		{
			public ILookup<ResourceSource, Resource> ItemsSource { get; }

			internal ResourceOutlineViewDataSource (ILookup<ResourceSource, Resource> itemsSource)
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
					case IGrouping<ResourceSource, Resource> kvp:
						return kvp.Count ();
					case Resource resource:
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
					case IGrouping<ResourceSource, Resource> kvp:
						element = kvp.ElementAt ((int)childIndex);
						break;
					case Resource resource:
						element = resource;
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
				case IGrouping<ResourceSource, Resource> kvp:
					return kvp.Any ();
				case Resource resource:
					return false;
				default:
					return false;
				}
			}
		}

		private const string ResourceSelectorColId = "ResourceSelectorColumn";

		public ResourceOutlineView resourceOutlineView;
		private readonly CreateBindingViewModel viewModel;

		internal BindingResourceSelectorControl (CreateBindingViewModel viewModel)
		{
			if (viewModel == null)
				throw new ArgumentNullException (nameof (viewModel));

			this.viewModel = viewModel;

			TranslatesAutoresizingMaskIntoConstraints = false;

			this.resourceOutlineView = new ResourceOutlineView ();
			this.resourceOutlineView.Activated += OnResourceOutlineViewSelected;

			var resourceColumn = new NSTableColumn (ResourceSelectorColId);
			this.resourceOutlineView.AddColumn (resourceColumn);

			// Set OutlineTableColumn or the arrows showing children/expansion will not be drawn
			this.resourceOutlineView.OutlineTableColumn = resourceColumn;

			// create a table view and a scroll view
			var outlineViewContainer = new NSScrollView {
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			// add the panel to the window
			outlineViewContainer.DocumentView = this.resourceOutlineView;
			AddSubview (outlineViewContainer);

			AddConstraints (new[] {
				NSLayoutConstraint.Create (outlineViewContainer, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this, NSLayoutAttribute.Top, 1f, 35f),
				NSLayoutConstraint.Create (outlineViewContainer, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this, NSLayoutAttribute.Left, 1f, 5f),
				NSLayoutConstraint.Create (outlineViewContainer, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this, NSLayoutAttribute.Width, 1f, -10f),
				NSLayoutConstraint.Create (outlineViewContainer, NSLayoutAttribute.Height, NSLayoutRelation.Equal,this, NSLayoutAttribute.Height, 1f, -40f),
			});

			viewModel.PropertyChanged += OnPropertyChanged;
		}

		private void OnResourceOutlineViewSelected (object sender, EventArgs e)
		{
			if (sender is ResourceOutlineView rov) {
				if (rov.SelectedRow != -1) {
					if (rov.ItemAtRow (rov.SelectedRow) is NSObjectFacade item) {
						if (item.Target is Resource resource) {
							this.viewModel.SelectedResource = resource;
						}
					}
				}
			}
		}

		private void OnPropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof (CreateBindingViewModel.ShowResourceSelector)) {
				Hidden = !this.viewModel.ShowResourceSelector;

				if (this.viewModel.ShowResourceSelector && this.viewModel.SourceResources != null) {
					this.resourceOutlineView.ItemsSource = this.viewModel.SourceResources.Value;
				};
			}
		}
	}
}
