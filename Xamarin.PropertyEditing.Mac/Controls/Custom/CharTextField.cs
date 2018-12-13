using System;
using AppKit;
using Foundation;

namespace Xamarin.PropertyEditing.Mac
{
	public class CharTextField : NSTextField
	{
		string cachedValueString;
		NSText CachedCurrentEditor { get; set; }

		public event EventHandler ValidatedEditingEnded;

		public override CoreGraphics.CGSize IntrinsicContentSize => new CoreGraphics.CGSize (30, 20);

		public CharTextField ()
		{
			BackgroundColor = NSColor.Clear;
			ControlSize = NSControlSize.Small;
			StringValue = string.Empty;
			TranslatesAutoresizingMaskIntoConstraints = false;
		}

		public override bool ShouldBeginEditing (NSText textObject)
		{
			CachedCurrentEditor = textObject;
			cachedValueString = textObject.Value;
			CachedCurrentEditor.Delegate = new CharValidationDelegate (this);
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

	public class CharValidationDelegate : NSTextViewDelegate
	{
		readonly CharTextField textField;

		public CharValidationDelegate (CharTextField textField)
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

			if (!char.TryParse (textObject.Value, out var result)) {
				textField.ResetInvalidInput ();
				AppKitFramework.NSBeep ();
				textField.ShouldEndEditing (textObject);
			} else {
				shouldEndEditing = textField.ShouldEndEditing (textObject);
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
