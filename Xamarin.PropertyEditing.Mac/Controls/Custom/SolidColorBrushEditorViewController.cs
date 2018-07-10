using System;
using System.ComponentModel;
using CoreGraphics;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	class SolidColorBrushEditorViewController : NotifyingViewController<BrushPropertyViewModel>
	{
		private SolidColorBrushEditor brushEditor;

		public SolidColorBrushEditorViewController ()
		{
			PreferredContentSize = new CGSize (300, 230);
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
			View = this.brushEditor = new SolidColorBrushEditor {
				ViewModel = ViewModel?.Solid
			};
		}
	}
}
