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

		NSTrackingArea trackingArea;

		public UnfocusableButton ()
		{
			Enabled = true;
			ImageScaling = NSImageScale.AxesIndependently;
			TranslatesAutoresizingMaskIntoConstraints = false;

			PropertyEditorPanel.ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;

			UpdateTheme ();
		}

		#region Overridden Methods
		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				PropertyEditorPanel.ThemeManager.ThemeChanged -= ThemeManager_ThemeChanged;
			}
		}

		public override void MouseDown (NSEvent theEvent)
		{
			switch (theEvent.Type) {
				case NSEventType.LeftMouseDown:
					NotifyMouseLeftDown ();
					break;

				case NSEventType.RightMouseDown:
					NotifyMouseRightDown ();
					break;
			}
		}

		public override void MouseEntered (NSEvent theEvent)
		{
			NotifyMouseEntered ();
		}

		public override void MouseExited (NSEvent theEvent)
		{
			NotifyMouseExited ();
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

		void ThemeManager_ThemeChanged (object sender, EventArgs e)
		{
			UpdateTheme ();
		}

		protected virtual void UpdateTheme ()
		{
			this.Appearance = PropertyEditorPanel.ThemeManager.CurrentAppearance;
		}
		#endregion
	}
}
