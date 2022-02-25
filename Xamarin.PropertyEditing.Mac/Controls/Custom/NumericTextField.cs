using System;
using AppKit;
using CoreGraphics;
using Foundation;
using ObjCRuntime;

namespace Xamarin.PropertyEditing.Mac
{
	internal class NumericTextField : PropertyTextField
	{
		public override bool CanBecomeKeyView { get { return Enabled; } }

		private NSText CachedCurrentEditor {
			get; set;
		}

		private string cachedValueString;

		public bool AllowNegativeValues {
			get; set;
		}

		public bool AllowRatios {
			get; set;
		}

		public ValidationType NumericMode {
			get; set;
		}

		public string FocusedFormat {
			get; set;
		}

		public string DisplayFormat
		{
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

		public ProxyResponder ProxyResponder
		{
			get {
				return Delegate is KeyUpDownDelegate viewDownDelegate ? viewDownDelegate.ProxyResponder : null;
			}
			set
			{
				if (Delegate is KeyUpDownDelegate keydown) {
					keydown.ProxyResponder = value;
				}
			}
		}

		public override CGSize IntrinsicContentSize => new CGSize(30, 20);

		public override bool ShouldBeginEditing (NSText textObject)
		{
			CachedCurrentEditor = textObject;
			this.cachedValueString = textObject.Value;

			if (AllowRatios)
				CachedCurrentEditor.Delegate = new RatioValidateDelegate (this);
			else {
				CachedCurrentEditor.Delegate = new NumericValidationDelegate (this);
			}
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
			this.StringValue = this.cachedValueString;
		}

		public static bool CheckIfNumber (string finalString, ValidationType mode, bool allowNegativeValues)
		{
			return mode == ValidationType.Decimal ?
				ValidateDecimal (finalString, allowNegativeValues) :
				ValidateInteger (finalString, allowNegativeValues);
		}

		public static bool ValidateDecimal (string finalString, bool allowNegativeValues)
		{
			//Checks parsing to number
			if (!double.TryParse (finalString, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.CurrentUICulture, out var value))
				return false;
			//Checks if needs to be possitive value
			if (!allowNegativeValues && value < 0)
				return false;
			//Checks a common validation
			return ViewModels.RatioViewModel.IsValidInput (finalString);
		}

		public static bool ValidateInteger (string finalString, bool allowNegativeValues)
		{
			//Checks parsing to number
			if (!int.TryParse (finalString, out var value))
				return false;
			//Checks if needs to be possitive value
			if (!allowNegativeValues && value < 0)
				return false;
			//Checks a common validation
			return ViewModels.RatioViewModel.IsValidInput (finalString);
		}

		public static bool CheckIfRatio (string finalString, ValidationType mode, bool allowNegativeValues)
		{
			var parts = finalString.Split (ViewModels.RatioViewModel.SplitSeparators, StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length == 2) {
				bool parsed = true;

				if (!CheckIfNumber (parts[0], mode, allowNegativeValues))
					parsed = false;

				if (!String.IsNullOrEmpty (parts[1]) && !CheckIfNumber (parts[1], mode, allowNegativeValues))
					parsed = false;

				if (parsed)
					return true;
			} else if (parts.Length == 1) { // We have a potential whole number, let's make sure
				if (CheckIfNumber (parts[0], mode, allowNegativeValues)) {
					return true;
				}
			}
			return false;
		}

		public override bool BecomeFirstResponder ()
		{
			if (FocusedFormat != null && Formatter is NSNumberFormatter numberFormatter) {
				numberFormatter.PositiveFormat = FocusedFormat;
			}
			return base.BecomeFirstResponder ();
		}

		public override void DidEndEditing (NSNotification notification)
		{
			if (DisplayFormat != null && Formatter is NSNumberFormatter numberFormatter) {
				numberFormatter.PositiveFormat = DisplayFormat;
			}
			base.DidEndEditing (notification);
		}
	}

	internal class KeyUpDownDelegate : DelegatedRowTextFieldDelegate
	{
		public event EventHandler<bool> KeyArrowUp;
		public event EventHandler<bool> KeyArrowDown;

		public override bool DoCommandBySelector (NSControl control, NSTextView textView, Selector commandSelector)
		{
			//if parent already handles command we break the event chain
			var parentHandlesCommand = base.DoCommandBySelector (control, textView, commandSelector);
			if (parentHandlesCommand)
			{
				return false;
			}

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

	internal abstract class TextViewValidationDelegate : NSTextViewDelegate
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

	internal class NumericValidationDelegate : TextViewValidationDelegate
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

	internal class RatioValidateDelegate : TextViewValidationDelegate
	{
		public RatioValidateDelegate (NumericTextField textField)
			: base (textField)
		{

		}

		protected override bool ValidateFinalString (string value)
		{
			if (NumericTextField.CheckIfRatio (value, ValidationType.Decimal, false) || NumericTextField.CheckIfNumber (value, ValidationType.Decimal, false))
				return true;
			return false;
		}
	}
}
