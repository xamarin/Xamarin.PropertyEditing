﻿using System;
using AppKit;
using CoreGraphics;
using Foundation;
using Xamarin.PropertyEditing.Drawing;
using Xamarin.PropertyEditing.Themes;

namespace Xamarin.PropertyEditing.Mac
{
	internal class NumericSpinEditor<T> : NumericSpinEditor
	{
	}

	internal class NumericSpinEditor : NSView, INSAccessibilityGroup
	{
		const int stepperSpace = 2;
		const int stepperWidth = 11;
		const int stepperTopHeight = 9;
		const int stepperBotHeight = 10;

		NumericTextField numericEditor;
		public NumericTextField NumericEditor {
			get { return numericEditor; }
		}

		UpSpinnerButton incrementButton;
		public UpSpinnerButton IncrementButton {
			get { return incrementButton; }
		}

		DownSpinnerButton decrementButton;
		public DownSpinnerButton DecrementButton {
			get { return decrementButton; }
		}

		protected bool editing;

		public event EventHandler ValueChanged;
		public event EventHandler EditingEnded;

		public ValidationType NumericMode {
			get { return numericEditor.NumericMode; }
			set {
				this.numericEditor.NumericMode = value;
				Reset ();
			}
		}

		public string PlaceholderString {
			get { return ((NSTextFieldCell)numericEditor.Cell).PlaceholderString; }
			set { ((NSTextFieldCell)numericEditor.Cell).PlaceholderString = value; }
		}

		public override CGSize IntrinsicContentSize {
			get {
				var baseSize = numericEditor.IntrinsicContentSize;
				return new CGSize (baseSize.Width + 20, baseSize.Height);
			}
		}

		public NSColor BackgroundColor {
			get { return numericEditor.BackgroundColor; }
			set { numericEditor.BackgroundColor = value; }
		}

		public override nfloat BaselineOffsetFromBottom {
			get { return numericEditor.BaselineOffsetFromBottom; }
		}

		public int Digits {
			get { return (int)formatter.MaximumFractionDigits; }
			set { formatter.MaximumFractionDigits = value; }
		}

		public double Value {
			get { return numericEditor.DoubleValue; }
			set { SetValue (value); }
		}

		public string StringValue
		{
			get { return numericEditor.StringValue; }
			set { SetValue (value); }
		}

		public double MinimumValue {
			get { return formatter.Minimum.DoubleValue; }
			set {
				formatter.Minimum = new NSNumber (value);
			}
		}

		public double MaximumValue {
			get { return formatter.Maximum.DoubleValue; }
			set {
				formatter.Maximum = new NSNumber (value);
			}
		}

		double incrementValue = 1.0f;
		public double IncrementValue {
			get { return incrementValue; }
			set { incrementValue = value; }
		}

		public string FocusedFormat
		{
			get { return numericEditor?.FocusedFormat; }
			set { numericEditor.FocusedFormat = value; }
		}

		public string DisplayFormat
		{
			get { return numericEditor?.DisplayFormat; }
			set { numericEditor.DisplayFormat = value; }
		}

		public bool Enabled {
			get { return numericEditor.Enabled; }
			set {
				numericEditor.Enabled = value;
				incrementButton.Enabled = value;
				decrementButton.Enabled = value;
			}
		}

		NSNumberFormatter formatter;
		public NSNumberFormatter Formatter {
			get { return formatter; }
			set {
				formatter = value;
				numericEditor.Formatter = formatter;
			}
		}

		public bool IsIndeterminate {
			get { return !string.IsNullOrEmpty (numericEditor.StringValue); }
			set {
				if (value)
					numericEditor.StringValue = string.Empty;
			}
		}

		public bool Editable {
			get { return numericEditor.Editable; }
			set {
				numericEditor.Editable = value;
				incrementButton.Enabled = value;
				decrementButton.Enabled = value;
			}
		}

		public NSNumberFormatterStyle NumberStyle {
			get { return formatter.NumberStyle; }
			set { formatter.NumberStyle = value; }
		}

		public bool AllowRatios
		{
			get { return numericEditor.AllowRatios; }
			set { numericEditor.AllowRatios = value; }
		}

		public bool AllowNegativeValues
		{
			get { return numericEditor.AllowNegativeValues; }
			set { numericEditor.AllowNegativeValues = value; }
		}

		public bool IsUpdatingMultiple { get; set; }

		public virtual void Reset ()
		{
		}

		public NumericSpinEditor ()
		{
			TranslatesAutoresizingMaskIntoConstraints = false;
			var controlSize = NSControlSize.Small;

			incrementButton = new UpSpinnerButton ();

			decrementButton = new DownSpinnerButton ();

			formatter = new NSNumberFormatter {
				FormatterBehavior = NSNumberFormatterBehavior.Version_10_4,
				Locale = NSLocale.CurrentLocale,
				MaximumFractionDigits = 15,
				Maximum = double.MaxValue,
				Minimum = double.MinValue,
				NumberStyle = NSNumberFormatterStyle.Decimal,
				UsesGroupingSeparator = false 
			};
			if (DisplayFormat != null) formatter.PositiveFormat = DisplayFormat;

			numericEditor = new NumericTextField {
				Alignment = NSTextAlignment.Right,
				TranslatesAutoresizingMaskIntoConstraints = false,
				Font = NSFont.FromFontName (PropertyEditorControl.DefaultFontName, PropertyEditorControl.DefaultFontSize),
				ControlSize = controlSize,
				Formatter = formatter
			};

			incrementButton.OnMouseLeftDown += (sender, e) => { IncrementNumericValue (); };
			decrementButton.OnMouseLeftDown += (sender, e) => { DecrementNumericValue (); };

			numericEditor.KeyArrowUp += (sender, e) => { IncrementNumericValue (e); };
			numericEditor.KeyArrowDown += (sender, e) => { DecrementNumericValue (e); };

			numericEditor.ValidatedEditingEnded += OnEditingEnded;

			AddSubview (numericEditor);
			AddSubview (incrementButton);
			AddSubview (decrementButton);

			this.DoConstraints (new[] {
				numericEditor.ConstraintTo (this, (n, c) => n.Width == c.Width - (stepperWidth + stepperSpace + 1)),
				numericEditor.ConstraintTo (this, (n, c) => n.Height == PropertyEditorControl.DefaultControlHeight - 3),

				incrementButton.ConstraintTo (numericEditor, (s, n) => s.Left == n.Right + stepperSpace),
				incrementButton.ConstraintTo (numericEditor, (s, n) => s.Top == n.Top),
				incrementButton.ConstraintTo (numericEditor, (s, n) => s.Width == stepperWidth),
				incrementButton.ConstraintTo (numericEditor, (s, n) => s.Height == stepperTopHeight),

				decrementButton.ConstraintTo (numericEditor, (s, n) => s.Left == n.Right + stepperSpace),
				decrementButton.ConstraintTo (numericEditor, (s, n) => s.Top == n.Top + stepperTopHeight),
				decrementButton.ConstraintTo (numericEditor, (s, n) => s.Width == stepperWidth),
				decrementButton.ConstraintTo (numericEditor, (s, n) => s.Height == stepperBotHeight),
			});

			PropertyEditorPanel.ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;

			UpdateTheme ();
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				PropertyEditorPanel.ThemeManager.ThemeChanged -= ThemeManager_ThemeChanged;
			}
		}

		void ThemeManager_ThemeChanged (object sender, EventArgs e)
		{
			UpdateTheme ();
		}

		protected void UpdateTheme ()
		{
			this.Appearance = PropertyEditorPanel.ThemeManager.CurrentAppearance;
		}

		virtual protected void OnEditingEnded (object sender, EventArgs e)
		{
			if (!editing) {
				editing = true;
				SetValue (numericEditor.StringValue);
				EditingEnded?.Invoke (this, EventArgs.Empty);
				editing = false;
			}
		}

		void SetValue (string value)
		{
			if (numericEditor.StringValue != value) {
				numericEditor.StringValue = value;
			}
		}

		public void SetValue (double value)
		{
			SetValue (value.ToString ());
		}

		protected void NotifyingValueChanged (EventArgs eventArgs)
		{
			if (!IsUpdatingMultiple) {
				ValueChanged?.Invoke (this, eventArgs);
			}
		}

		public void IncrementNumericValue (bool shiftPressed = false)
		{
			if (!editing) {
				editing = true;
				SetIncrementOrDecrementValue (shiftPressed ? 10 * incrementValue : incrementValue);
				editing = false;
			}
		}

		public void DecrementNumericValue (bool shiftPressed = false)
		{
			if (!editing) {
				editing = true;
				SetIncrementOrDecrementValue (-(shiftPressed ? 10 * incrementValue : incrementValue));
				editing = false;
			}
		}

		virtual protected void SetIncrementOrDecrementValue (double incDevValue)
		{
			// Constrain our value to our Min/Max before we set it
			var newValue = Clamp (numericEditor.DoubleValue + incDevValue);

			SetValue (newValue);
			NotifyingValueChanged (EventArgs.Empty);
		}

		public double Clamp (double value)
		{
			return (double)Decimal.Round ((decimal)(value < MinimumValue ? MinimumValue : value > MaximumValue ? MaximumValue : value), Digits);
		}
	}
}
