using System;
using AppKit;

namespace Xamarin.PropertyEditing.Mac
{
	internal class AutoClosePopOver : NSPopover
	{
		public bool CloseOnEnter { get; internal set; }

		public AutoClosePopOver () : base ()
		{
			Behavior = NSPopoverBehavior.Semitransient;
			CloseOnEnter = true;
		}

		public override void KeyUp (NSEvent theEvent)
		{
			// If Enter Kit, close the pop-up
			if (theEvent.KeyCode == 36 && CloseOnEnter) {
				this.Close ();
			}
		}
	}
}
