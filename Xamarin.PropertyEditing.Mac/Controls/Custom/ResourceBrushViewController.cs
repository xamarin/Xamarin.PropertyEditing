using System;
using System.ComponentModel;
using AppKit;
using CoreGraphics;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class ResourceBrushViewController : NotifyingViewController<BrushPropertyViewModel>
	{
		private ResourceOutlineView resourceSelector;
		private readonly ResourceBrushPropertyViewDelegate viewDelegate;

		public ResourceBrushViewController (IHostResourceProvider hostResources)
		{
			PreferredContentSize = new CGSize (430, 230);
			this.viewDelegate = new ResourceBrushPropertyViewDelegate (hostResources);
		}

		private Resource resource;
		public override void OnPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName) {
				case nameof (BrushPropertyViewModel.Resource):
					if (resource == ViewModel.Resource)
						return;

					resource = ViewModel?.Resource;
					UpdateSelection ();
					break;
				case nameof (BrushPropertyViewModel.Solid):
					if (resourceSelector != null)
						resourceSelector.ViewModel = ViewModel?.ResourceSelector;
					break;
				case nameof (BrushPropertyViewModel.Value):
					break;
			}
		}

		private void UpdateSelection ()
		{
			if (resourceSelector == null)
				return;

			var source = resourceSelector.DataSource as ResourceDataSource;
			if (source == null || ViewModel == null)
				return;

			nint index = -1;
			if (ViewModel.Resource != null && source.TryGetFacade (ViewModel?.Resource, out var facade)) {
				index = resourceSelector.RowForItem (facade);
			}

			if (index < 0)
				resourceSelector.DeselectAll (null);
			else
				resourceSelector.SelectRow (index, false);
		}

		public override void OnViewModelChanged (BrushPropertyViewModel oldModel)
		{
			base.OnViewModelChanged (oldModel);
			if (resourceSelector != null) {
				viewDelegate.ViewModel = ViewModel;
				resourceSelector.ViewModel = ViewModel?.ResourceSelector;
			}
		}

		public new NSScrollView View {
			get => base.View as NSScrollView;
			set => base.View = (value as NSScrollView);
		}

		public override void LoadView ()
		{
			viewDelegate.ViewModel = ViewModel;
			this.resourceSelector = new ResourceOutlineView {
				Delegate = viewDelegate,
			};

			// create a table view and a scroll view
			var tableContainer = new NSScrollView (new CGRect (10, PreferredContentSize.Height - 210, PreferredContentSize.Width, PreferredContentSize.Height)) {
				AutoresizingMask = NSViewResizingMask.WidthSizable | NSViewResizingMask.HeightSizable
			};

			// add the panel to the window
			tableContainer.DocumentView = this.resourceSelector;
			View = tableContainer;

			if (ViewModel != null) {
				resourceSelector.ViewModel = ViewModel?.ResourceSelector;
			}
		}

		public void ReloadData()
		{
			this.resourceSelector.ReloadData ();
		}
	}
}
