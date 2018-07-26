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

		public ValidationType NumericMode {
			get; set;
		}

		public event EventHandler<bool> KeyArrowUp;
		public event EventHandler<bool> KeyArrowDown;
		public event EventHandler ValidatedEditingEnded;

		public NumericTextField ()
		{
			AllowNegativeValues = true;

			var keyUpDownDelegate = new KeyUpDownDelegate ();
			keyUpDownDelegate.KeyArrowUp += (sender, e) => { OnKeyArrowUp (e); };
			keyUpDownDelegate.KeyArrowDown += (sender, e) => { OnKeyArrowDown (e); };
			Delegate = keyUpDownDelegate;
		}

		public override bool ShouldBeginEditing (NSText textObject)
		{
			CachedCurrentEditor = textObject;
			cachedValueString = textObject.Value;

			CachedCurrentEditor.Delegate = new NumericValidationDelegate (this);

			return true;
		}

		protected virtual void OnKeyArrowUp (bool shiftPressed)
		{
			KeyArrowUp?.Invoke (this, shiftPressed);
		}

		protected virtual void OnKeyArrowDown (bool shiftPressed)
		{
			KeyArrowDown?.Invoke (this, shiftPressed);
		}

		public virtual void NotifyValidatedEditingEnded ()
		{
			ValidatedEditingEnded?.Invoke (this, EventArgs.Empty);
		}

		public virtual void ResetInvalidInput ()
		{
			this.StringValue = cachedValueString;
		}

		public static bool CheckIfNumber (string finalString, ValidationType mode, bool allowNegativeValues)
		{
			return mode == ValidationType.Decimal ?
				ValidateDecimal (finalString, allowNegativeValues) :
				ValidateInteger (finalString, allowNegativeValues);
		}

		public static bool ValidateDecimal (string finalString, bool allowNegativeValues)
		{
			double value;
			//Checks parsing to number
			if (!double.TryParse (finalString, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.CurrentUICulture, out value))
				return false;
			//Checks if needs to be possitive value
			if (!allowNegativeValues && value < 0)
				return false;

			return true;
		}

		public static bool ValidateInteger (string finalString, bool allowNegativeValues)
		{
			int value;
			//Checks parsing to number
			if (!int.TryParse (finalString, out value))
				return false;
			//Checks if needs to be possitive value
			if (!allowNegativeValues && value < 0)
				return false;

			return true;
		}
	}

	class KeyUpDownDelegate : NSTextFieldDelegate
	{
		public event EventHandler<bool> KeyArrowUp;
		public event EventHandler<bool> KeyArrowDown;

		public override bool DoCommandBySelector (NSControl control, NSTextView textView, Selector commandSelector)
		{
			switch (commandSelector.Name) {
				case "moveUp:":
					OnKeyArrowUp ();
					break;
				case "moveDown:":
					OnKeyArrowDown ();
					break;
				case "moveUpAndModifySelection:":
					OnKeyArrowUp (true);
					break;
				case "moveDownAndModifySelection:":
					OnKeyArrowDown (true);
					break;
				default:
					return false;
			}

			return true;
		}

		protected virtual void OnKeyArrowUp (bool shiftPressed = false)
		{
			KeyArrowUp?.Invoke (this, shiftPressed);
		}

		protected virtual void OnKeyArrowDown (bool shiftPressed = false)
		{
			KeyArrowDown?.Invoke (this, shiftPressed);
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
			bool shouldEndEditing = false;
			if (!ValidateFinalString (textObject.Value)) {
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
				NumericTextField.ValidateDecimal (value, TextField.AllowNegativeValues) :
				NumericTextField.ValidateInteger (value, TextField.AllowNegativeValues);
		}
	}
}
