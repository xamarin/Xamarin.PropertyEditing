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
		}

		protected override void OnPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName) {
				case nameof (BrushPropertyViewModel.Solid):
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

		public override void LoadView ()
		{
			View = brushEditor = new SolidColorBrushEditor ();
			if (ViewModel != null)
				brushEditor.ViewModel = ViewModel?.Solid;
		}
	}
}
