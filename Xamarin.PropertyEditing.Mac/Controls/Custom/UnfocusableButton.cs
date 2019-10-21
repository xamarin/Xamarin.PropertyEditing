using System;
using AppKit;

namespace Xamarin.PropertyEditing.Mac
{
	public class UnfocusableButton : NSImageView
	{
		public event EventHandler OnMouseEntered;
		public event EventHandler OnMouseExited;
		public event EventHandler OnMouseLeftDown;
		public event EventHandler OnMouseRightDown;

		private NSTrackingArea trackingArea;

        internal IHostResourceProvider HostResources { get; }

		private readonly string imageName;

		internal UnfocusableButton (IHostResourceProvider hostResources, string imageName)
		{
			if (hostResources == null)
				throw new ArgumentNullException (nameof (hostResources));

			if (imageName == null)
				throw new ArgumentNullException (nameof (imageName));

			HostResources = hostResources;
			this.imageName = imageName;
		
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
			if (trackingArea == null) {
				var options = NSTrackingAreaOptions.MouseEnteredAndExited | NSTrackingAreaOptions.ActiveAlways;

				trackingArea = new NSTrackingArea (this.Bounds, options, this, null);

				AddTrackingArea (trackingArea);
			}
		}

		public sealed override void ViewDidChangeEffectiveAppearance ()
		{
			base.ViewDidChangeEffectiveAppearance ();

			AppearanceChanged ();
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
			OnMouseLeftDown?.Invoke (this, EventArgs.Empty);
		}

		private void NotifyMouseRightDown ()
		{
			OnMouseRightDown?.Invoke (this, EventArgs.Empty);
		}

		protected virtual void AppearanceChanged ()
		{
			Image = HostResources.GetNamedImage (this.imageName);
		}
		#endregion
	}
}
