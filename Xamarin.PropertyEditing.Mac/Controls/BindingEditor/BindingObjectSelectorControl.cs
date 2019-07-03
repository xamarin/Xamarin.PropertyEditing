using System;
using System.Collections.Generic;
using AppKit;
using Foundation;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class BindingObjectSelectorControl 
		: NotifyingView<CreateBindingViewModel>
	{
		private ObjectOutlineView objectOutlineView;

		private const string ObjectSelectorColId = "ObjectSelectorColumn";

		internal BindingObjectSelectorControl (CreateBindingViewModel viewModel)
		{
			if (viewModel == null)
				throw new ArgumentNullException (nameof (viewModel));

			ViewModel = viewModel;

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
				NSLayoutConstraint.Create (outlineViewContainer, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this, NSLayoutAttribute.Top, 1f, 28f),
				NSLayoutConstraint.Create (outlineViewContainer, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this, NSLayoutAttribute.Width, 1f, 0f),
				NSLayoutConstraint.Create (outlineViewContainer, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, this, NSLayoutAttribute.CenterX, 1, 0),
				NSLayoutConstraint.Create (outlineViewContainer, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal,this, NSLayoutAttribute.Bottom, 1f, 0f),
			});

			viewModel.PropertyChanged += OnPropertyChanged;
		}

		public override void OnPropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof (CreateBindingViewModel.ShowObjectSelector)) {
				Hidden = !ViewModel.ShowObjectSelector;

				if (ViewModel.ShowObjectSelector && ViewModel.ObjectElementRoots != null) {
					this.objectOutlineView.ItemsSource = ViewModel.ObjectElementRoots.Value;
				};
			}
		}

		private void OnObjectOutlineViewSelected (object sender, EventArgs e)
		{
			if (sender is ObjectOutlineView rov) {
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
