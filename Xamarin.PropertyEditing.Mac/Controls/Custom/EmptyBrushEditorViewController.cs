using AppKit;
using CoreGraphics;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class EmptyBrushEditorViewController : NotifyingViewController<BrushPropertyViewModel>
	{
		public EmptyBrushEditorViewController ()
		{
			PreferredContentSize = new CGSize (PreferredContentSizeWidth, PreferredContentSizeHeight);
		}

		private NSButton brushEditor;

		public override void LoadView ()
		{
			View = this.brushEditor = new NSButton {
				Bordered = false,
				Title = Properties.Resources.NoBrush,
				Enabled = false
			};
		}
	}
}
