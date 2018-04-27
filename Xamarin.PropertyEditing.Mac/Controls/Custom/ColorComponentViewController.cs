using System;
using System.ComponentModel;
using AppKit;
using CoreGraphics;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	class ColorComponentViewController : PropertyViewController<SolidBrushViewModel>
	{
		ColorComponentEditor editor;

		public ColorComponentViewController (ChannelEditorType type) : base ()
		{
			PreferredContentSize = new CGSize (200, 200);
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

		public ChannelEditorType EditorType { get; }

        public override void LoadView ()
        {
			View = editor = new ColorComponentEditor (this.EditorType);
        }
    }
}
