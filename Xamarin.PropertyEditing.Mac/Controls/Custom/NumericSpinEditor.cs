using System;

using AppKit;
using CoreGraphics;
using Foundation;
using ObjCRuntime;

namespace Xamarin.PropertyEditing.Mac
{
	internal class NumericSpinEditor<T>
		: NumericSpinEditor
	{
		public NumericSpinEditor (IHostResourceProvider hostResources)
			: base (hostResources)
		{
		}
	}

	internal class NumericSpinEditor
		: NSView, INSAccessibilityGroup
	{
		private NumericTextField numericEditor;
		public NumericTextField NumericEditor {
			get { return this.numericEditor; }
		}

		private SpinnerButton incrementButton;
		public SpinnerButton IncrementButton {
			get { return this.incrementButton; }
		}

		private SpinnerButton decrementButton;
		public SpinnerButton DecrementButton {
			get { return this.decrementButton; }
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
			get { return ((NSTextFieldCell)this.numericEditor.Cell).PlaceholderString; }
			set { ((NSTextFieldCell)this.numericEditor.Cell).PlaceholderString = value; }
		}

		public override CGSize IntrinsicContentSize {
			get {
				var baseSize = this.numericEditor.IntrinsicContentSize;
				return new CGSize (baseSize.Width + 20, baseSize.Height);
			}
		}

		public NSColor BackgroundColor {
			get { return this.numericEditor.BackgroundColor; }
			set { this.numericEditor.BackgroundColor = value; }
		}

		public override nfloat BaselineOffsetFromBottom {
			get { return this.numericEditor.BaselineOffsetFromBottom; }
		}

		public int Digits {
			get { return (int)this.formatter.MaximumFractionDigits; }
			set { this.formatter.MaximumFractionDigits = value; }
		}

		public double Value {
			get { return this.numericEditor.DoubleValue; }
			set { SetValue (value); }
		}

		public string StringValue
		{
			get { return this.numericEditor.StringValue; }
			set { SetValue (value); }
		}

		public double MinimumValue {
			get { return this.formatter.Minimum.DoubleValue; }
			set { this.formatter.Minimum = new NSNumber (value); }
		}

		public double MaximumValue {
			get { return this.formatter.Maximum.DoubleValue; }
			set { this.formatter.Maximum = new NSNumber (value); }
		}

		private double incrementValue = 1.0f;
		public double IncrementValue {
			get { return this.incrementValue; }
			set { this.incrementValue = value; }
		}

		public string FocusedFormat
		{
			get { return this.numericEditor?.FocusedFormat; }
			set { this.numericEditor.FocusedFormat = value; }
		}

		public string DisplayFormat
		{
			get { return this.numericEditor?.DisplayFormat; }
			set { this.numericEditor.DisplayFormat = value; }
		}

		public bool Enabled {
			get { return this.numericEditor.Enabled; }
			set {
				this.numericEditor.Enabled = value;
				this.incrementButton.Enabled = value;
				this.decrementButton.Enabled = value;
			}
		}

		private NSNumberFormatter formatter;
		public NSNumberFormatter Formatter {
			get { return this.formatter; }
			set {
				this.formatter = value;
				this.numericEditor.Formatter = this.formatter;
			}
		}

		public bool IsIndeterminate {
			get { return !string.IsNullOrEmpty (this.numericEditor.StringValue); }
			set {
				if (value)
					this.numericEditor.StringValue = string.Empty;
			}
		}

		public bool Editable {
			get { return this.numericEditor.Editable; }
			set {
				this.numericEditor.Editable = value;
				this.incrementButton.Enabled = value;
				this.decrementButton.Enabled = value;
			}
		}

		public NSNumberFormatterStyle NumberStyle {
			get { return this.formatter.NumberStyle; }
			set { this.formatter.NumberStyle = value; }
		}

		public bool AllowRatios {
			get { return this.numericEditor.AllowRatios; }
			set { this.numericEditor.AllowRatios = value; }
		}

		public bool AllowNegativeValues {
			get { return this.numericEditor.AllowNegativeValues; }
			set { this.numericEditor.AllowNegativeValues = value; }
		}

		public override bool AccessibilityEnabled {
			get { return this.numericEditor.AccessibilityEnabled; }
			set { this.numericEditor.AccessibilityEnabled = value; }
		}

		public override string AccessibilityTitle {
			get { return this.numericEditor.AccessibilityTitle; }
			set { this.numericEditor.AccessibilityTitle = value; }
		}

		public ProxyResponder ProxyResponder
		{
			get => this.numericEditor.ProxyResponder;
			set => this.numericEditor.ProxyResponder = value;
		}

		public virtual void Reset ()
		{
		}

		public NumericSpinEditor (IHostResourceProvider hostResources)
		{
			if (hostResources == null)
				throw new ArgumentNullException (nameof (hostResources));

			this.hostResources = hostResources;
			TranslatesAutoresizingMaskIntoConstraints = false;

			this.incrementButton = new SpinnerButton (this.hostResources, isUp: true) {
				TranslatesAutoresizingMaskIntoConstraints = false
			};

			this.decrementButton = new SpinnerButton (this.hostResources, isUp: false) {
				TranslatesAutoresizingMaskIntoConstraints = false
			};

			this.formatter = new NSNumberFormatter {
				FormatterBehavior = NSNumberFormatterBehavior.Version_10_4,
				Locale = NSLocale.CurrentLocale,
				MaximumFractionDigits = 15,
				Maximum = double.MaxValue,
				Minimum = double.MinValue,
				NumberStyle = NSNumberFormatterStyle.Decimal,
				UsesGroupingSeparator = false 
			};

			if (DisplayFormat != null)
				this.formatter.PositiveFormat = DisplayFormat;

			this.numericEditor = new NumericTextField {
				Alignment = NSTextAlignment.Right,
				TranslatesAutoresizingMaskIntoConstraints = false,
				Font = NSFont.SystemFontOfSize (NSFont.SystemFontSizeForControlSize (NSControlSize.Small)),
				ControlSize = NSControlSize.Small,
				Formatter = this.formatter
			};

			this.incrementButton.OnMouseLeftDown += (sender, e) => { IncrementNumericValue (); };
			this.decrementButton.OnMouseLeftDown += (sender, e) => { DecrementNumericValue (); };

			this.numericEditor.KeyArrowUp += (sender, e) => { IncrementNumericValue (e); };
			this.numericEditor.KeyArrowDown += (sender, e) => { DecrementNumericValue (e); };

			this.numericEditor.ValidatedEditingEnded += OnEditingEnded;

			AddSubview (this.numericEditor);
			AddSubview (this.incrementButton);
			AddSubview (this.decrementButton);

			AddConstraints (new[] {
				NSLayoutConstraint.Create (this.numericEditor, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this, NSLayoutAttribute.Left, 1f, 0),
				NSLayoutConstraint.Create (this.numericEditor, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 18),

				NSLayoutConstraint.Create (this.incrementButton, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.numericEditor, NSLayoutAttribute.Top, 1f, 0f),
				NSLayoutConstraint.Create (this.incrementButton, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this.numericEditor, NSLayoutAttribute.Right, 1f, StepperSpace),
				NSLayoutConstraint.Create (this.incrementButton, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this, NSLayoutAttribute.Right, 1f, 0),
				NSLayoutConstraint.Create (this.incrementButton, NSLayoutAttribute.Width, NSLayoutRelation.Equal, 1f, StepperWidth),
				NSLayoutConstraint.Create (this.incrementButton, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, StepperTopHeight),

				NSLayoutConstraint.Create (this.decrementButton, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.numericEditor,  NSLayoutAttribute.Top, 1f, StepperTopHeight),
				NSLayoutConstraint.Create (this.decrementButton, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this, NSLayoutAttribute.Right, 1f, 0),
				NSLayoutConstraint.Create (this.decrementButton, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this.numericEditor,  NSLayoutAttribute.Right, 1f, StepperSpace),
				NSLayoutConstraint.Create (this.decrementButton, NSLayoutAttribute.Width, NSLayoutRelation.Equal, 1f, StepperWidth),
				NSLayoutConstraint.Create (this.decrementButton, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, StepperBotHeight),
			});
		}

		virtual protected void OnEditingEnded (object sender, EventArgs e)
		{
			if (!this.editing) {
				this.editing = true;
				SetValue (numericEditor.StringValue);
				EditingEnded?.Invoke (this, EventArgs.Empty);
				NotifyingValueChanged (EventArgs.Empty);
				this.editing = false;
			}
		}

		void SetValue (string value)
		{
			if (this.numericEditor.StringValue != value) {
				this.numericEditor.StringValue = value;
			}
		}

		public void SetValue (double value)
		{
			SetValue (value.ToString ());
		}

		protected void NotifyingValueChanged (EventArgs eventArgs)
		{
			ValueChanged?.Invoke (this, eventArgs);
		}

		public void IncrementNumericValue (bool shiftPressed = false)
		{
			if (!this.editing) {
				this.editing = true;
				SetIncrementOrDecrementValue (shiftPressed ? 10 * this.incrementValue : this.incrementValue);
				this.editing = false;
			}
		}

		public void DecrementNumericValue (bool shiftPressed = false)
		{
			if (!editing) {
				this.editing = true;
				SetIncrementOrDecrementValue (-(shiftPressed ? 10 * this.incrementValue : this.incrementValue));
				this.editing = false;
			}
		}

		virtual protected void SetIncrementOrDecrementValue (double incDevValue)
		{
			// Constrain our value to our Min/Max before we set it
			var newValue = Clamp (this.numericEditor.DoubleValue + incDevValue);

			SetValue (newValue);
			NotifyingValueChanged (EventArgs.Empty);
		}

		public double Clamp (double value)
		{
			return (double)Decimal.Round ((decimal)(value < MinimumValue ? MinimumValue : value > MaximumValue ? MaximumValue : value), Digits);
		}

		private const int StepperSpace = 2;
		private const int StepperWidth = 11;
		private const int StepperTopHeight = 9;
		private const int StepperBotHeight = 10;

		private readonly IHostResourceProvider hostResources;
	}
}
