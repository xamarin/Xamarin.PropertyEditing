using System;
using AppKit;

namespace Xamarin.PropertyEditing.Mac
{
	internal class AutoClosePopOver : NSPopover
	{
		private IHostResourceProvider hostResources;

		public bool CloseOnEnter { get; internal set; }

		public AutoClosePopOver (IHostResourceProvider hostResources) : base ()
		{
			if (hostResources == null)
				throw new ArgumentNullException (nameof (hostResources));

			this.hostResources = hostResources;

			Behavior = NSPopoverBehavior.Semitransient;
			CloseOnEnter = true;

			this.SetAppearance (this.hostResources.GetVibrantAppearance (EffectiveAppearance));
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
