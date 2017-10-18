using System;
using AppKit;
using Foundation;
using ObjCRuntime;

namespace Xamarin.PropertyEditing.Mac
{
	public class NumericTextField : NSTextField
	{
		NSText CachedCurrentEditor {
			get; set;
		}

		string cachedValueString;

		public bool AllowNegativeValues {
			get; set;
		}

		public bool AllowRatios {
			get; set;
		}

		public ValidationType NumericMode {
			get; set;
		}

		public event EventHandler KeyArrowUp;
		public event EventHandler KeyArrowDown;
		public event EventHandler ValidatedEditingEnded;

		public NumericTextField ()
		{
			AllowNegativeValues = true;

			var keyUpDownDelegate = new KeyUpDownDelegate ();
			keyUpDownDelegate.KeyArrowUp += (sender, e) => { OnKeyArrowUp (); };
			keyUpDownDelegate.KeyArrowDown += (sender, e) => { OnKeyArrowDown (); };
			Delegate = keyUpDownDelegate;
		}

		public override bool ShouldBeginEditing (NSText fieldEditor)
		{
			CachedCurrentEditor = fieldEditor;
			cachedValueString = fieldEditor.Value;

			if (AllowRatios)
				CachedCurrentEditor.Delegate = new RatioValidateDelegate (this);
			else {
				CachedCurrentEditor.Delegate = new NumericValidationDelegate (this);
			}
			return true;
		}

		protected virtual void OnKeyArrowUp ()
		{
			var handler = KeyArrowUp;
			if (handler != null) {
				handler (this, EventArgs.Empty);
			}
		}

		protected virtual void OnKeyArrowDown ()
		{
			var handler = KeyArrowDown;
			if (handler != null) {
				handler (this, EventArgs.Empty);
			}
		}

		public virtual void RaiseValidatedEditingEnded ()
		{
			var handler = ValidatedEditingEnded;
			if (handler != null)
				handler (this, EventArgs.Empty);
		}

		public virtual void ResetInvalidInput ()
		{
			this.StringValue = cachedValueString;
		}

	}

	class KeyUpDownDelegate : NSTextFieldDelegate
	{
		public event EventHandler KeyArrowUp;
		public event EventHandler KeyArrowDown;

		public override bool DoCommandBySelector (NSControl control, NSTextView textView, Selector commandSelector)
		{
			switch (commandSelector.Name) {
				case "moveUp:":
					OnKeyArrowUp ();
					break;
				case "moveDown:":
					OnKeyArrowDown ();
					break;
				default:
					return false;
			}

			return false;
		}

		protected virtual void OnKeyArrowUp ()
		{
			var handler = KeyArrowUp;
			if (handler != null) {
				handler (this, EventArgs.Empty);
			}
		}

		protected virtual void OnKeyArrowDown ()
		{
			var handler = KeyArrowDown;
			if (handler != null) {
				handler (this, EventArgs.Empty);
			}
		}
	}

	public abstract class TextViewValidationDelegate : NSTextViewDelegate
	{
		protected NumericTextField TextField {
			get; set;
		}

		public TextViewValidationDelegate (NumericTextField textField)
		{
			TextField = textField;
		}

		public override bool TextShouldBeginEditing (NSText textObject)
		{
			return TextField.ShouldBeginEditing (textObject);
		}

		public override bool TextShouldEndEditing (NSText textObject)
		{
			if (!ValidateFinalString (TextField.StringValue)) {
				TextField.ResetInvalidInput ();
				AppKitFramework.NSBeep ();
				return false;
			}
			return TextField.ShouldEndEditing (textObject);
		}

		public override void TextDidEndEditing (NSNotification notification)
		{
			TextField.RaiseValidatedEditingEnded ();
		}

		protected abstract bool ValidateFinalString (string value);
	}

	public class NumericValidationDelegate : TextViewValidationDelegate
	{
		public NumericValidationDelegate (NumericTextField textField)
			: base (textField)
		{

		}

		protected override bool ValidateFinalString (string value)
		{
			return TextField.NumericMode == ValidationType.Decimal ?
				FieldValidation.ValidateDecimal (value, TextField.AllowNegativeValues) :
				FieldValidation.ValidateInteger (value, TextField.AllowNegativeValues);
		}
	}

	public class RatioValidateDelegate : TextViewValidationDelegate
	{
		public RatioValidateDelegate (NumericTextField textField)
			: base (textField)
		{

		}

		protected override bool ValidateFinalString (string value)
		{
			return FieldValidation.ValidateRatio (value, ValidationType.Decimal, TextField.AllowNegativeValues);
		}
	}
}
