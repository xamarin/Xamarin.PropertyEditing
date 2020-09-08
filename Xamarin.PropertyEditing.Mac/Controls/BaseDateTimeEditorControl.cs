using System;
using AppKit;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal abstract class BaseDateTimeEditorControl<T> : PropertyEditorControl<PropertyViewModel<T>>
	{
		public NSDatePicker DatePicker { get; }

		public BaseDateTimeEditorControl (IHostResourceProvider hostResources)
			: base (hostResources)
		{
			DatePicker = new NSDatePicker {
				ControlSize = NSControlSize.Small,
				DatePickerElements = NSDatePickerElementFlags.HourMinuteSecond | NSDatePickerElementFlags.YearMonthDateDay,
				DatePickerStyle = NSDatePickerStyle.TextFieldAndStepper,
				Font = NSFont.SystemFontOfSize (NSFont.SystemFontSizeForControlSize (NSControlSize.Small)),
				TimeZone = Foundation.NSTimeZone.FromAbbreviation ("UTC"),
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			// update the value on keypress
			DatePicker.Activated += Editor_Activated;

			AddSubview (DatePicker);

			AddConstraints (new[] {
				NSLayoutConstraint.Create (DatePicker, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, this,  NSLayoutAttribute.CenterY, 1f, 0f),
				NSLayoutConstraint.Create (DatePicker, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Leading, 1f, 0f),
				NSLayoutConstraint.Create (DatePicker, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Width, 1f, 0),
				NSLayoutConstraint.Create (DatePicker, NSLayoutAttribute.Height, NSLayoutRelation.Equal, this, NSLayoutAttribute.Height, 1f,  -6f),
			});
		}

		protected abstract void Editor_Activated (object sender, EventArgs e);

		public override NSView FirstKeyView => DatePicker;
		public override NSView LastKeyView => DatePicker;

		protected override void SetEnabled ()
		{
			DatePicker.Enabled = ViewModel.Property.CanWrite;
		}

		protected override void UpdateAccessibilityValues ()
		{
			DatePicker.AccessibilityTitle = string.Format (Properties.Resources.AccessibilityDateTime, ViewModel.Property.Name);
			DatePicker.Enabled = DatePicker.Enabled;
		}

		protected override void Dispose (bool disposing)
		{
			DatePicker.Activated -= Editor_Activated;
			base.Dispose (disposing);
		}
	}
}