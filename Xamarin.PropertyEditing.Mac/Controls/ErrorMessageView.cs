using System;
using System.Collections;
using AppKit;

namespace Xamarin.PropertyEditing.Mac
{
	internal class ErrorMessageView : BaseEditorControl
	{
		NSTextField ErrorMessages;

		public ErrorMessageView (IEnumerable errors)
		{
			if (errors == null)
				throw new ArgumentNullException ("errors");

			ErrorMessages = new NSTextField {
				TranslatesAutoresizingMaskIntoConstraints = false,
				BackgroundColor = NSColor.Clear,
				Editable = false,
			};

			foreach (var error in errors) {
				ErrorMessages.StringValue += error + "\n";
			}

			AddSubview (ErrorMessages);

			this.DoConstraints (new[] {
				ErrorMessages.ConstraintTo (this, (s, c) => s.Width == c.Width - 5),
				ErrorMessages.ConstraintTo (this, (s, c) => s.Height == c.Height - 5),
			});
		}
	}
}
