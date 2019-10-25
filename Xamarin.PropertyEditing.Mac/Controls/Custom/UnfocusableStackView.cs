using AppKit;

namespace Xamarin.PropertyEditing.Mac
{
	class UnfocusableStackView : NSStackView
	{
		public override bool AcceptsFirstResponder () => false;
	}
}