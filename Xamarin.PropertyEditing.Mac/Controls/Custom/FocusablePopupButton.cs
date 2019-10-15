using System;
using AppKit;
using CoreGraphics;

namespace Xamarin.PropertyEditing.Mac
{
	internal class FocusablePopUpButton : NSPopUpButton
	{
		public override bool CanBecomeKeyView { get { return Enabled; } }

		public FocusablePopUpButton ()
		{
			Cell.LineBreakMode = NSLineBreakMode.TruncatingMiddle;
		}

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
