using System;
using System.ComponentModel;
using System.Linq;
using AppKit;
using CoreGraphics;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	class MaterialBrushEditorViewController : NotifyingViewController<BrushPropertyViewModel>
	{
		public MaterialBrushEditorViewController ()
		{
			PreferredContentSize = new CGSize (200, 230);
		}

		private MaterialView materialEditor;

		public override void OnPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName) {
				case nameof (BrushPropertyViewModel.Value):
				case nameof (BrushPropertyViewModel.MaterialDesign):
					if (this.materialEditor != null)
						this.materialEditor.ViewModel = ViewModel;
					break;
			}
		}

		public override void OnViewModelChanged (BrushPropertyViewModel oldModel)
		{
			if (ViewLoaded && materialEditor != null)
				this.materialEditor.ViewModel = ViewModel;
		}

		public override void LoadView ()
		{
			View = materialEditor = new MaterialView {
				ViewModel = ViewModel
			};
		}
	}
}
