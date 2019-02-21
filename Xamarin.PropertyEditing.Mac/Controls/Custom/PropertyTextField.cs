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
