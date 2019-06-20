using System;
using AppKit;
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
				Font = NSFont.SystemFontOfSize (NSFont.SystemFontSizeForControlSize (NSControlSize.Small)),
				TranslatesAutoresizingMaskIntoConstraints = false
			};

			// update the value on keypress
			this.datePicker.Activated += Editor_Activated;

			AddSubview (this.datePicker);

			AddConstraints (new[] {
				NSLayoutConstraint.Create (this.datePicker, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, this,  NSLayoutAttribute.CenterY, 1f, 0f),
				NSLayoutConstraint.Create (this.datePicker, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Leading, 1f, 0f),
				NSLayoutConstraint.Create (this.datePicker, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Width, 1f, 0),
				NSLayoutConstraint.Create (this.datePicker, NSLayoutAttribute.Height, NSLayoutRelation.Equal, this, NSLayoutAttribute.Height, 1f,  -6f),
			});

			AppearanceChanged ();
		}

		private void Editor_Activated (object sender, EventArgs e)
		{
			ViewModel.Value = this.datePicker.DateValue.ToDateTime ();
		}

		public override NSView FirstKeyView => this.datePicker;
		public override NSView LastKeyView => this.datePicker;

		protected override void AppearanceChanged ()
		{
			base.AppearanceChanged ();

			this.datePicker.TextColor = HostResources.GetNamedColor (NamedResources.ForegroundColor);
		}

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
			this.datePicker.AccessibilityTitle = string.Format (Properties.Resources.AccessibilityDateTime, ViewModel.Property.Name);
			this.datePicker.Enabled = this.datePicker.Enabled;
		}

		protected override void Dispose (bool disposing)
		{
			this.datePicker.Activated -= Editor_Activated;
			base.Dispose (disposing);
		}
	}
}
