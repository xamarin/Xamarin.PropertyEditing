using Foundation;
using AppKit;

namespace Xamarin.PropertyEditing.Mac
{
	internal class FocusableComboBox : NSComboBox
	{
		public override bool BecomeFirstResponder ()
		{
			var willBecomeFirstResponder = base.BecomeFirstResponder ();
			if (willBecomeFirstResponder) {
				ScrollRectToVisible (Bounds);
			}
			return willBecomeFirstResponder;
		}
	}
}