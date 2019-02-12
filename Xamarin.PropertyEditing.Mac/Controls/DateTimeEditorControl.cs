using System;
using System.Collections;
using AppKit;
using CoreGraphics;
using Foundation;
using Xamarin.PropertyEditing.Mac.Resources;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class DateTimeEditorControl : PropertyEditorControl<PropertyViewModel<DateTime>>
	{
		private readonly NSDatePicker datePicker;

		public DateTimeEditorControl (IHostResourceProvider hostResources)
			: base (hostResources)
		{
			this.datePicker = new NSDatePicker {
				ControlSize = NSControlSize.Small,
				DatePickerElements = NSDatePickerElementFlags.HourMinuteSecond | NSDatePickerElementFlags.YearMonthDateDay,
				DatePickerStyle = NSDatePickerStyle.TextFieldAndStepper,
				Font = NSFont.FromFontName (DefaultFontName, DefaultFontSize),
				TranslatesAutoresizingMaskIntoConstraints = false
			};

			// update the value on keypress
			this.datePicker.Activated += Editor_Activated;

			AddSubview (this.datePicker);

			AddConstraints (new[] {
				NSLayoutConstraint.Create (this.datePicker, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Top, 1f, 1f),
				NSLayoutConstraint.Create (this.datePicker, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Left, 1f, 0f),
				NSLayoutConstraint.Create (this.datePicker, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Width, 1f, 0f),
				NSLayoutConstraint.Create (this.datePicker, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, DefaultControlHeight - 6),
			});
		}

		private void Editor_Activated (object sender, EventArgs e) => ViewModel.Value = this.datePicker.DateValue.ToDateTime ();

		public override NSView FirstKeyView => this.datePicker;
		public override NSView LastKeyView => this.datePicker;

		protected override void UpdateValue ()
		{
			this.datePicker.DateValue = ViewModel.Value.ToNSDate ();
		}

		protected override void SetEnabled ()
		{
			this.datePicker.Enabled = ViewModel.Property.CanWrite;
		}

		protected override void UpdateAccessibilityValues ()
		{
			this.datePicker.AccessibilityTitle = string.Format (LocalizationResources.AccessibilityDateTime, ViewModel.Property.Name);
			this.datePicker.Enabled = this.datePicker.Enabled;
		}

		protected override void Dispose (bool disposing)
		{
			this.datePicker.Activated -= Editor_Activated;
			base.Dispose (disposing);
		}
	}
}
