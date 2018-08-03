using AppKit;
using CoreGraphics;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	class EmptyBrushEditorViewController : NotifyingViewController<BrushPropertyViewModel>
	{
		public EmptyBrushEditorViewController ()
		{
			PreferredContentSize = new CGSize (430, 230);
		}

		private NSButton brushEditor;

		public override void LoadView ()
		{
			View = brushEditor = new NSButton {
				Bordered = false,
				Title = Properties.Resources.NoBrush,
				Enabled = false
			};
		}
	}
}
