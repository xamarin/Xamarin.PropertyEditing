using System;
using System.ComponentModel;
using CoreGraphics;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class SolidColorBrushEditorViewController
		: NotifyingViewController<BrushPropertyViewModel>
	{
		public SolidColorBrushEditorViewController (IHostResourceProvider hostResources)
		{
			if (hostResources == null)
				throw new ArgumentNullException (nameof (hostResources));

			this.hostResources = hostResources;
			PreferredContentSize = new CGSize (PreferredContentSizeWidth, PreferredContentSizeHeight);
		}

		public override void OnPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName) {
				case nameof (BrushPropertyViewModel.Solid):
					if (this.brushEditor != null)
						this.brushEditor.ViewModel = ViewModel.Solid;
					break;
				case nameof (BrushPropertyViewModel.Value):
					if (this.brushEditor != null)
						this.brushEditor.ViewModel = ViewModel.Solid;
					break;
			}
		}

		public override void OnViewModelChanged (BrushPropertyViewModel oldModel)
		{
			if (this.brushEditor != null)
				this.brushEditor.ViewModel = ViewModel?.Solid;
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			if (ViewModel != null)
				this.brushEditor.ViewModel = ViewModel?.Solid;
		}

		public override void LoadView ()
		{
			View = this.brushEditor = new SolidColorBrushEditor (this.hostResources) {
				ViewModel = ViewModel?.Solid
			};
		}

		private readonly IHostResourceProvider hostResources;
		private SolidColorBrushEditor brushEditor;
	}
}
