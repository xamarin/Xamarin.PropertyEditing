using System;
using AppKit;

namespace Xamarin.PropertyEditing.Mac
{
	internal class AutoClosePopOver : NSPopover
	{
		public AutoClosePopOver () : base ()
		{
			Behavior = NSPopoverBehavior.Semitransient;
		}

		public override void KeyUp (NSEvent theEvent)
		{
			// If Enter Kit, close the pop-up
			if (theEvent.KeyCode == 36) {
				this.Close ();
			}
		}
	}
}
