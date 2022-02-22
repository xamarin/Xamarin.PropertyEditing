using System;
using AppKit;

namespace Xamarin.PropertyEditing.Mac
{
	internal class FocusableBooleanButton : ProxyResponderButton
	{
		public override bool CanBecomeKeyView { get { return Enabled; } }

		public FocusableBooleanButton ()
		{
			AllowsExpansionToolTips = true;
			AllowsMixedState = true;
			Cell.LineBreakMode = NSLineBreakMode.TruncatingTail;
			Cell.UsesSingleLineMode = true;
			ControlSize = NSControlSize.Small;
			Font = NSFont.SystemFontOfSize (NSFont.SystemFontSizeForControlSize (NSControlSize.Small));
			Title = string.Empty;
			TranslatesAutoresizingMaskIntoConstraints = false;

			SetButtonType (NSButtonType.Switch);
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
