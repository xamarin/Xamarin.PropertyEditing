using System;
using System.ComponentModel;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	class SolidColorBrushEditorViewController : PropertyViewController<BrushPropertyViewModel>
	{
		SolidColorBrushEditor brushEditor;

		public SolidColorBrushEditorViewController ()
		{
			//PreferredMinimumSize = new CGSize (200, 200);
		}

		protected override void OnPropertyChanged (object sender, PropertyChangedEventArgs e)
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

		protected override void OnViewModelChanged (BrushPropertyViewModel oldModel)
		{
			if (brushEditor != null)
				brushEditor.ViewModel = ViewModel?.Solid;
		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
			//View.Appearance = PropertyEditorPanel.ThemeManager.CurrentAppearance;
			if (ViewModel != null)
				brushEditor.ViewModel = ViewModel?.Solid;
        }

        public override void LoadView ()
		{
			View = brushEditor = new SolidColorBrushEditor {
				//Appearance = PropertyEditorPanel.ThemeManager.CurrentAppearance,
				ViewModel = ViewModel?.Solid
			};
		}
	}
}
