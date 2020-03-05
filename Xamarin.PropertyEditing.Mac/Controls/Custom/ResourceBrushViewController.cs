using System;
using System.ComponentModel;
using AppKit;
using CoreGraphics;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class ResourceBrushViewController
		: NotifyingViewController<BrushPropertyViewModel>
	{
		private ResourceOutlineView resourceSelector;
		private readonly ResourceBrushPropertyViewDelegate viewDelegate;

		public ResourceBrushViewController (IHostResourceProvider hostResources)
		{
			PreferredContentSize = new CGSize (PreferredContentSizeWidth, PreferredContentSizeHeight);
			this.viewDelegate = new ResourceBrushPropertyViewDelegate (hostResources);
		}

		private Resource resource;
		public override void OnPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName) {
				case nameof (BrushPropertyViewModel.Resource):
					if (this.resource == ViewModel.Resource)
						return;

					this.resource = ViewModel?.Resource;
					UpdateSelection ();
					break;
				case nameof (BrushPropertyViewModel.Solid):
					if (this.resourceSelector != null)
						this.resourceSelector.ViewModel = ViewModel?.ResourceSelector;
					break;
				case nameof (BrushPropertyViewModel.Value):
					break;
			}
		}

		private void UpdateSelection ()
		{
			if (this.resourceSelector == null)
				return;

			var source = this.resourceSelector.DataSource as ResourceDataSource;
			if (source == null || ViewModel == null)
				return;

			nint index = -1;
			if (ViewModel.Resource != null && source.TryGetFacade (ViewModel?.Resource, out var facade)) {
				index = this.resourceSelector.RowForItem (facade);
			}

			if (index < 0)
				this.resourceSelector.DeselectAll (null);
			else
				this.resourceSelector.SelectRow (index, false);
		}

		public override void OnViewModelChanged (BrushPropertyViewModel oldModel)
		{
			base.OnViewModelChanged (oldModel);
			if (this.resourceSelector != null) {
				this.viewDelegate.ViewModel = ViewModel;
				this.resourceSelector.ViewModel = ViewModel?.ResourceSelector;
			}
		}

		public new NSScrollView View {
			get => base.View as NSScrollView;
			set => base.View = (value as NSScrollView);
		}

		public override void LoadView ()
		{
			this.viewDelegate.ViewModel = ViewModel;
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
				this.resourceSelector.ViewModel = ViewModel?.ResourceSelector;
			}
		}

		public void ReloadData()
		{
			this.resourceSelector.ReloadData ();
		}
	}
}
