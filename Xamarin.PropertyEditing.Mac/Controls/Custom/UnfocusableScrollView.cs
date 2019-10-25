using AppKit;

namespace Xamarin.PropertyEditing.Mac
{
	class UnfocusableScrollView : NSScrollView
	{
		public override bool AcceptsFirstResponder () => false;
	}
}