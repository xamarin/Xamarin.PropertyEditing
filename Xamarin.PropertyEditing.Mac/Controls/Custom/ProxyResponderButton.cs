using AppKit;

namespace Xamarin.PropertyEditing.Mac
{
	internal class ProxyResponderButton : NSButton
	{
		public ProxyResponder ProxyResponder { get; set; }

		public override void KeyDown (NSEvent theEvent)
		{
			switch (theEvent.KeyCode) {
			case (int)NSKey.Tab:
				if (ProxyResponder != null) {
					if (theEvent.ModifierFlags.HasFlag(NSEventModifierMask.ShiftKeyMask)) {
						ProxyResponder.PreviousResponder ();
					} else {
						ProxyResponder.NextResponder ();
					}
					return;
				}
				break;
			}
			base.KeyDown (theEvent);
		}
	}
}
