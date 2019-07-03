using System;
using System.Collections.Generic;
using System.Linq;
using AppKit;
using Foundation;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class BindingResourceSelectorControl 
		: NotifyingView<CreateBindingViewModel>
	{
		private const string ResourceSelectorColId = "ResourceSelectorColumn";

		public BindingResourceOutlineView resourceOutlineView;

		internal BindingResourceSelectorControl (CreateBindingViewModel viewModel)
		{
			if (viewModel == null)
				throw new ArgumentNullException (nameof (viewModel));

			ViewModel = viewModel;

			TranslatesAutoresizingMaskIntoConstraints = false;

			this.resourceOutlineView = new BindingResourceOutlineView ();
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
				NSLayoutConstraint.Create (outlineViewContainer, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this, NSLayoutAttribute.Top, 1f, 28f),
				NSLayoutConstraint.Create (outlineViewContainer, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this, NSLayoutAttribute.Width, 1f, 0f),
				NSLayoutConstraint.Create (outlineViewContainer, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, this, NSLayoutAttribute.CenterX, 1, 0),
				NSLayoutConstraint.Create (outlineViewContainer, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal,this, NSLayoutAttribute.Bottom, 1f, 0f),
			});

			viewModel.PropertyChanged += OnPropertyChanged;
		}

		public override void OnPropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof (CreateBindingViewModel.ShowResourceSelector)) {
				Hidden = !ViewModel.ShowResourceSelector;

				if (ViewModel.ShowResourceSelector && ViewModel.SourceResources != null) {
					this.resourceOutlineView.ItemsSource = ViewModel.SourceResources.Value;
				};
			}
		}

		private void OnResourceOutlineViewSelected (object sender, EventArgs e)
		{
			if (sender is BindingResourceOutlineView rov) {
				if (rov.SelectedRow != -1) {
					if (rov.ItemAtRow (rov.SelectedRow) is NSObjectFacade item) {
						if (item.Target is Resource resource) {
							ViewModel.SelectedResource = resource;
						}
					}
				}
			}
		}
	}
}
