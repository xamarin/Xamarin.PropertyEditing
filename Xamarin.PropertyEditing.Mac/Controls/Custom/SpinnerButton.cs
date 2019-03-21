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

		public SpinnerButton (IHostResourceProvider hostResource, bool isUp)
		{
			if (hostResource == null)
				throw new ArgumentNullException (nameof (hostResource));

			this.hostResources = hostResource;
			this.imageBase += (isUp) ? "up" : "down";

			ViewDidChangeEffectiveAppearance ();
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

		public override void ViewDidChangeEffectiveAppearance ()
		{
			this.image = this.hostResources.GetNamedImage (this.imageBase);
			this.mouseOverImage = this.hostResources.GetNamedImage (this.imageBase + "-focus-blue");

			UpdateImage ();
		}

		private readonly IHostResourceProvider hostResources;
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
