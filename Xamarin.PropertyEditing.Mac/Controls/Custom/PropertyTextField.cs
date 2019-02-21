using System;
using AppKit;

namespace Xamarin.PropertyEditing.Mac
{
	internal class PropertyTextField : NSTextField
	{
		public PropertyTextField ()
		{
			AllowsExpansionToolTips = true;
			Cell.LineBreakMode = NSLineBreakMode.TruncatingTail;
			Cell.UsesSingleLineMode = true;
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

	internal class PropertyTextFieldCell : NSTextFieldCell
	{
		public PropertyTextFieldCell ()
		{
			LineBreakMode = NSLineBreakMode.TruncatingTail;
			UsesSingleLineMode = true;
		}
	}
}
