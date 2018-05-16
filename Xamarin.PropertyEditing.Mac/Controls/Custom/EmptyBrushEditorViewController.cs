using AppKit;
using CoreGraphics;
using Xamarin.PropertyEditing.Drawing;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	class EmptyBrushEditorViewController : NotifyingViewController<BrushPropertyViewModel>
	{
		public EmptyBrushEditorViewController ()
		{
			PreferredContentSize = new CGSize (100, 100);
		}

		NSButton brushEditor;

		public override void LoadView ()
		{
			View = brushEditor = new NSButton {
				Title = "Edit"
			};

			brushEditor.Activated += (o, e) => {
				ViewModel.SelectedBrushType = CommonBrushType.Solid;
			};
		}
	}
}
