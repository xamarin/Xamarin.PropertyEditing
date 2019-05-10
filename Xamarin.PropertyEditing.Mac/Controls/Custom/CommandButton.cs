using System;
using System.Windows.Input;
using AppKit;

namespace Xamarin.PropertyEditing.Mac
{
	internal class CommandButton : NSButton
	{
		private ICommand command;

		public event EventHandler OnMouseEntered;
		public event EventHandler OnMouseExited;
		public event EventHandler OnMouseLeftDown;
		public event EventHandler OnMouseRightDown;

		private NSTrackingArea trackingArea;
		private bool commandFired;

		public ICommand Command
		{
			get { return this.command; }
			set {
				if (this.command != null)
					this.command.CanExecuteChanged -= CanExecuteChanged;

				this.command = value;

				if (this.command != null)
					this.command.CanExecuteChanged += CanExecuteChanged;
			}
		}

		public CommandButton ()
		{
			Activated += ExecuteCommand;
		}

		private void ExecuteCommand (object sender, EventArgs e)
		{
			if (Enabled && !this.commandFired) {
				this.command?.Execute (null);
				return;
			}
			this.commandFired = false;
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
				base.MouseDown (theEvent);
			}
		}

		public override void MouseEntered (NSEvent theEvent)
		{
			if (Enabled) {
				NotifyMouseEntered ();
				base.MouseEntered (theEvent);
			}
		}

		public override void MouseExited (NSEvent theEvent)
		{
			if (Enabled) {
				NotifyMouseExited ();
				base.MouseExited (theEvent);
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
		private void CanExecuteChanged (object sender, EventArgs e)
		{
			if (sender is ICommand cmd)
				Enabled = cmd.CanExecute (null);
		}

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
			ExecuteCommand (this, EventArgs.Empty);
			OnMouseLeftDown?.Invoke (this, EventArgs.Empty);
			this.commandFired = true;
		}

		private void NotifyMouseRightDown ()
		{
			OnMouseRightDown?.Invoke (this, EventArgs.Empty);
		}
		#endregion
	}

	internal class FocusableCommandButton : CommandButton
	{
		public override bool CanBecomeKeyView { get { return Enabled; } }

		public FocusableCommandButton ()
		{
			AllowsExpansionToolTips = true;
			AllowsMixedState = true;
			Cell.LineBreakMode = NSLineBreakMode.TruncatingTail;
			Cell.UsesSingleLineMode = true;
			ControlSize = NSControlSize.Small;
			Font = NSFont.SystemFontOfSize (NSFont.SystemFontSizeForControlSize (NSControlSize.Small));
			Title = string.Empty;
			TranslatesAutoresizingMaskIntoConstraints = false;
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
