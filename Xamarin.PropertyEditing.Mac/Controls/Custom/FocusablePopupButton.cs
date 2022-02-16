using System;
using AppKit;
using CoreGraphics;

namespace Xamarin.PropertyEditing.Mac
{
	internal class FocusablePopUpButton : NSPopUpButton
	{
		public override bool CanBecomeKeyView { get { return Enabled; } }

		public ProxyRowResponder ResponderProxy { get; set; }

		public FocusablePopUpButton ()
		{
			Cell.LineBreakMode = NSLineBreakMode.TruncatingMiddle;
		}

		public override void KeyDown (NSEvent theEvent)
		{
			switch (theEvent.KeyCode) {
			case (int)NSKey.Tab:
				if (ResponderProxy != null) {
					if (theEvent.ModifierFlags.HasFlag(NSEventModifierMask.ShiftKeyMask)) {
						ResponderProxy.PreviousResponder ();
					} else {
						ResponderProxy.NextResponder ();
					}
				}
				return;
			}
			base.KeyDown (theEvent);
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
