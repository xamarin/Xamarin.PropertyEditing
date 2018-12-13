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
		protected CharTextField TextField { get; set; }

		public CharValidationDelegate (CharTextField textField)
		{
			TextField = textField;
		}

		public override bool TextShouldBeginEditing (NSText textObject)
		{
			return TextField.ShouldBeginEditing (textObject);
		}

		public override bool TextShouldEndEditing (NSText textObject)
		{
			var shouldEndEditing = false;

			if (!char.TryParse (textObject.Value, out var result)) {
				TextField.ResetInvalidInput ();
				AppKitFramework.NSBeep ();
				TextField.ShouldEndEditing (textObject);
			} else {
				shouldEndEditing = TextField.ShouldEndEditing (textObject);
			}

			return shouldEndEditing;
		}

		public override void TextDidEndEditing (NSNotification notification)
		{
			TextField.NotifyValidatedEditingEnded ();
			TextField.DidEndEditing (notification);
		}
	}
}
