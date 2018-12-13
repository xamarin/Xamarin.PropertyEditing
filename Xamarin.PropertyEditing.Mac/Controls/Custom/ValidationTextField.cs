using System;
using AppKit;
using Foundation;

namespace Xamarin.PropertyEditing.Mac
{
	public interface IValidationTextDelegate
	{
		bool IsValid (NSText textObject);
	}

	public class ValidationTextField : NSTextField
	{
		string cachedValueString;
		NSText CachedCurrentEditor { get; set; }

		public event EventHandler ValidatedEditingEnded;

		public override CoreGraphics.CGSize IntrinsicContentSize => new CoreGraphics.CGSize (30, 20);

		readonly IValidationTextDelegate validationTextDelegate;

		public ValidationTextField (IValidationTextDelegate validationTextDelegate)
		{
			this.validationTextDelegate = validationTextDelegate;
		}

		public override bool ShouldBeginEditing (NSText textObject)
		{
			CachedCurrentEditor = textObject;
			cachedValueString = textObject.Value;
			CachedCurrentEditor.Delegate = new CustomValidationDelegate (this);
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

		public class CustomValidationDelegate : NSTextViewDelegate
		{
			readonly ValidationTextField textField;

			public CustomValidationDelegate (ValidationTextField textField)
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

				if (textField.validationTextDelegate.IsValid (textObject)) {
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
