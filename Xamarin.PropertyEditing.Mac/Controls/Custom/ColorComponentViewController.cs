using System;
using System.ComponentModel;
using AppKit;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	class ColorComponentViewController : PropertyViewController<SolidBrushViewModel>
	{
		ColorComponentEditor editor;

		public ColorComponentViewController (EditorType type) : base ()
		{
			this.EditorType = type;
		}

		public override void OnViewModelChanged (SolidBrushViewModel oldModel)
		{
			base.OnViewModelChanged (oldModel);
			editor.ViewModel = ViewModel;
		}

		public override void OnPropertyChanged (object sender, PropertyChangedEventArgs args)
		{
			switch (args.PropertyName) {
				case nameof (SolidBrushViewModel.Color):
					break;
			}
		}

		public EditorType EditorType { get; }

        public override void LoadView ()
        {
			View = editor = new ColorComponentEditor (this.EditorType);
        }
    }
}
