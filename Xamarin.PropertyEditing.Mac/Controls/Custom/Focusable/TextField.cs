using System;
using AppKit;

namespace Xamarin.PropertyEditing.Mac.Controls.Custom.Focusable
{
	public class TextField : NSTextField, IFocusable
	{
		public event EventHandler GainedFocus;

		public void TriggerFocus (object sender) => GainedFocus?.Invoke (sender, EventArgs.Empty);

		public override bool BecomeFirstResponder ()
		{
			var firstResponder = base.BecomeFirstResponder ();
			if (firstResponder) {
				TriggerFocus (this);
			}
			return firstResponder;
		}
	}
}
