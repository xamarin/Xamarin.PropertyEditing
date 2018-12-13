using System;
using AppKit;
using Foundation;

namespace Xamarin.PropertyEditing.Mac
{
	public class TimeSpanTextField : NSTextField
	{
		string cachedValueString;
		NSText cachedCurrentEditor;

		public event EventHandler ValidatedEditingEnded;

		public override CoreGraphics.CGSize IntrinsicContentSize => new CoreGraphics.CGSize (30, 20);

		public TimeSpanTextField ()
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
			cachedCurrentEditor.Delegate = new TimeSpanValidationDelegate (this);
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
	}

	public class TimeSpanValidationDelegate : NSTextViewDelegate
	{
		readonly TimeSpanTextField textField;

		public TimeSpanValidationDelegate (TimeSpanTextField textField)
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

			if (TimeSpan.TryParse (textObject.Value, System.Globalization.CultureInfo.CurrentUICulture, out var result)) {
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
