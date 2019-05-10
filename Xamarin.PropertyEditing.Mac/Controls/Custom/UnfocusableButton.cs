using System;
using AppKit;

namespace Xamarin.PropertyEditing.Mac
{
	public abstract class UnfocusableButton : NSImageView
	{
		public event EventHandler OnMouseEntered;
		public event EventHandler OnMouseExited;
		public event EventHandler OnMouseLeftDown;
		public event EventHandler OnMouseRightDown;

		private NSTrackingArea trackingArea;

		public UnfocusableButton ()
		{
			Enabled = true;
			ImageScaling = NSImageScale.AxesIndependently;
		}

		#region Overridden Methods

		public override void MouseDown (NSEvent theEvent)
		{
			if (Enabled) {
				switch (theEvent.Type) {
				case NSEventType.LeftMouseDown:
					NotifyMouseLeftDown ();
					break;

				case NSEventType.RightMouseDown:
					NotifyMouseRightDown ();
					break;
				}
			}
		}

		public override void MouseEntered (NSEvent theEvent)
		{
			if (Enabled) {
				NotifyMouseEntered ();
			}
		}

		public override void MouseExited (NSEvent theEvent)
		{
			if (Enabled) {
				NotifyMouseExited ();
			}
		}

		public override void UpdateTrackingAreas ()
		{
			base.UpdateTrackingAreas ();

			// Add tracking so our MouseEntered and MouseExited get called.
			if (this.trackingArea == null) {
				NSTrackingAreaOptions options = NSTrackingAreaOptions.MouseEnteredAndExited | NSTrackingAreaOptions.ActiveAlways;

				this.trackingArea = new NSTrackingArea (Bounds, options, this, null);

				AddTrackingArea (this.trackingArea);
			}
		}
		#endregion


		#region Local Methods
		private void NotifyMouseEntered ()
		{
			OnMouseEntered?.Invoke (this, EventArgs.Empty);
		}

		private void NotifyMouseExited ()
		{
			OnMouseExited?.Invoke (this, EventArgs.Empty);
		}

		private void NotifyMouseLeftDown ()
		{
			//this.command?.Execute (null);
			OnMouseLeftDown?.Invoke (this, EventArgs.Empty);
		}

		private void NotifyMouseRightDown ()
		{
			OnMouseRightDown?.Invoke (this, EventArgs.Empty);
		}
		#endregion
	}
}
