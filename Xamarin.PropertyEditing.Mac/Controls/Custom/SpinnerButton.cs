using System;
using AppKit;
using CoreGraphics;
using ObjCRuntime;

namespace Xamarin.PropertyEditing.Mac
{
	internal class SpinnerButton
		: UnfocusableButton
	{
		public bool AcceptsUserInteraction { get; set; } = true;

		public override NSView HitTest (CGPoint aPoint)
		{
			if (!AcceptsUserInteraction) {
				return null;
			}

			return base.HitTest (aPoint);
		}

		public SpinnerButton (IHostResourceProvider hostResource, bool isUp) : base (hostResource, "") // Blank because it is set up before AppearanceChanged is called
		{
			this.imageBase += (isUp) ? "up" : "down";

			AppearanceChanged ();
		}

		public override void MouseExited (NSEvent theEvent)
		{
			this.isMouseOver = false;
			UpdateImage ();
		}

		public override void MouseEntered (NSEvent theEvent)
		{
			this.isMouseOver = true;
			UpdateImage ();
		}

		protected override void AppearanceChanged ()
		{
			this.image = HostResources.GetNamedImage (this.imageBase);
			this.mouseOverImage = HostResources.GetNamedImage (this.imageBase + "-focus-blue");

			UpdateImage ();
		}

		private bool isMouseOver;
		private string imageBase = "pe-stepper-";

		private NSImage image;
		private NSImage mouseOverImage;

		private void UpdateImage ()
		{
			Image = Enabled ? (this.isMouseOver) ? this.mouseOverImage : this.image : this.image;
		}
	}
}
