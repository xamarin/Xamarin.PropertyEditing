using System;
using AppKit;
using Foundation;

namespace Xamarin.PropertyEditing.Mac
{
	public class DateTimeTextField : NSTextField
	{
		string cachedValueString;
		NSText cachedCurrentEditor;

		public event EventHandler ValidatedEditingEnded;

		public override CoreGraphics.CGSize IntrinsicContentSize => new CoreGraphics.CGSize (30, 20);

		public DateTimeTextField ()
		{
			BackgroundColor = NSColor.Clear;
			ControlSize = NSControlSize.Small;
			StringValue = string.Empty;
			TranslatesAutoresizingMaskIntoConstraints = false;
		}

		public override bool ShouldBeginEditing (NSText textObject)
		{
			cachedCurrentEditor = textObject;
			cachedValueString = textObject.Value;
			cachedCurrentEditor.Delegate = new DateTimeValidationDelegate (this);
			return true;
		}

		public virtual void NotifyValidatedEditingEnded ()
		{
			ValidatedEditingEnded?.Invoke (this, EventArgs.Empty);
		}

		public virtual void ResetInvalidInput ()
		{
			this.StringValue = cachedValueString;
		}

		class DateTimeValidationDelegate : NSTextViewDelegate
		{
			readonly DateTimeTextField textField;

			public DateTimeValidationDelegate (DateTimeTextField textField)
			{
				this.textField = textField;
			}

			public override bool TextShouldBeginEditing (NSText textObject)
			{
				return textField.ShouldBeginEditing (textObject);
			}

			public override bool TextShouldEndEditing (NSText textObject)
			{
				var shouldEndEditing = false;

				if (DateTime.TryParse (textObject.Value, out DateTime dateValue)) {
					shouldEndEditing = textField.ShouldEndEditing (textObject);
				} else {
					textField.ResetInvalidInput ();
					AppKitFramework.NSBeep ();
					textField.ShouldEndEditing (textObject);
				}

				return shouldEndEditing;
			}

			public override void TextDidEndEditing (NSNotification notification)
			{
				textField.NotifyValidatedEditingEnded ();
				textField.DidEndEditing (notification);
			}
		}
	}
}
