using System;
using System.ComponentModel;
using CoreGraphics;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	class SolidColorBrushEditorViewController : PropertyViewController<BrushPropertyViewModel>
	{
		SolidColorBrushEditor brushEditor;

		public SolidColorBrushEditorViewController ()
		{
			PreferredContentSize = new CGSize (300, 230);
		}

		public override void OnPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName) {
				case nameof (BrushPropertyViewModel.Solid):
					if (brushEditor != null)
						brushEditor.ViewModel = ViewModel.Solid;
					break;
				case nameof (BrushPropertyViewModel.Value):
					if (brushEditor != null)
						brushEditor.ViewModel = ViewModel.Solid;
					break;
			}
		}

		public override void OnViewModelChanged (BrushPropertyViewModel oldModel)
		{
			if (brushEditor != null)
				brushEditor.ViewModel = ViewModel?.Solid;
		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

			if (ViewModel != null)
				brushEditor.ViewModel = ViewModel?.Solid;
        }

        public override void LoadView ()
		{
			View = brushEditor = new SolidColorBrushEditor {
				ViewModel = ViewModel?.Solid
			};
		}
	}
}
