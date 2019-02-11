using System;
using AppKit;
using CoreGraphics;

namespace Xamarin.PropertyEditing.Mac
{
	internal class CustomDatePicker : NSView
	{
		public event EventHandler Activated;

		private const int stepperSpace = 2;
		private const int stepperWidth = 11;
		private const int stepperLeftSeparation = -10;

		private const int stepperTopHeight = 9;
		private const int stepperBotHeight = 10;

		private NSDatePicker datePicker;
		public NSDatePicker DatePicker => this.datePicker;

		private readonly SpinnerButton incrementButton;
		private readonly SpinnerButton decrementButton;

		private readonly IHostResourceProvider hostResources;

		public DateTime DateTime
		{
			get => this.datePicker.DateValue.ToDateTime ();
			set => this.datePicker.DateValue = value.ToNSDate ();
		}

		public NSFont Font
		{
			get => this.datePicker.Font;
			set => this.datePicker.Font = value;
		}

		public bool Enabled
		{
			get => this.datePicker.Enabled;
			set
			{
				this.datePicker.Enabled = value;
				this.incrementButton.Enabled = value;
				this.decrementButton.Enabled = value;
			}
		}

		public CustomDatePicker (IHostResourceProvider hostResources)
		{
			this.hostResources = hostResources;
			TranslatesAutoresizingMaskIntoConstraints = false;

			this.datePicker = new NSDatePicker {
				DatePickerStyle = NSDatePickerStyle.TextFieldAndStepper,
				ControlSize = NSControlSize.Mini,
				DatePickerElements = NSDatePickerElementFlags.HourMinuteSecond | NSDatePickerElementFlags.YearMonthDateDay,
				TranslatesAutoresizingMaskIntoConstraints = false
			};

			//HACK: DatePicker doesn't allow modify the current spinners of the control
			//to fix this we overlap our current spinners leaving the current functionality of the native control
			this.incrementButton = new SpinnerButton (this.hostResources, isUp: true) {
				TranslatesAutoresizingMaskIntoConstraints = false,
				AcceptsUserInteraction = false
			};
			this.decrementButton = new SpinnerButton (this.hostResources, isUp: false) {
				TranslatesAutoresizingMaskIntoConstraints = false,
				AcceptsUserInteraction = false
			};

			this.datePicker.Activated += NumericEditor_Activated;

			AddSubview (this.datePicker);
			AddSubview (this.incrementButton);
			AddSubview (this.decrementButton);

			AddConstraints (new[] {
				NSLayoutConstraint.Create (this.datePicker, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, PropertyEditorControl.DefaultControlHeight - 3),

				NSLayoutConstraint.Create (this.incrementButton, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.datePicker,  NSLayoutAttribute.Top, 1f, 0f),
				NSLayoutConstraint.Create (this.incrementButton, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this.datePicker,  NSLayoutAttribute.Right, 1f, stepperLeftSeparation),
				NSLayoutConstraint.Create (this.incrementButton, NSLayoutAttribute.Width, NSLayoutRelation.Equal, 1f, stepperWidth),
				NSLayoutConstraint.Create (this.incrementButton, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, stepperTopHeight),

				NSLayoutConstraint.Create (this.decrementButton, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.datePicker,  NSLayoutAttribute.Top, 1f, stepperTopHeight),
				NSLayoutConstraint.Create (this.decrementButton, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this.datePicker,  NSLayoutAttribute.Right, 1f, stepperLeftSeparation),
				NSLayoutConstraint.Create (this.decrementButton, NSLayoutAttribute.Width, NSLayoutRelation.Equal, 1f, stepperWidth),
				NSLayoutConstraint.Create (this.decrementButton, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, stepperBotHeight),
			});
		}

		private void NumericEditor_Activated (object sender, EventArgs e) => Activated?.Invoke (this, EventArgs.Empty);

		protected override void Dispose (bool disposing)
		{
			this.datePicker.Activated -= NumericEditor_Activated;
			base.Dispose (disposing);
		}
	}
}
