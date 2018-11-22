using System;
using System.Collections;
using AppKit;
using CoreGraphics;
using Foundation;
using Xamarin.PropertyEditing.Drawing;
using Xamarin.PropertyEditing.Mac.Resources;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class DateTimeEditorControl : PropertyEditorControl<PropertyViewModel<CommonDateTime>>
	{
		internal NSDatePicker datePicker;

		public override NSView FirstKeyView => this.datePicker;
		public override NSView LastKeyView => this.datePicker;

		public DateTimeEditorControl ()
		{
			this.datePicker = new NSDatePicker (new CGRect (4, 0, 170, 24)) {
				DatePickerStyle = NSDatePickerStyle.TextFieldAndStepper,
			};

			this.datePicker.Activated += OnInputUpdated;

			AddSubview (this.datePicker);

			/* Some controle may require constraints this.DoConstraints (new[] {
				this.datePicker.ConstraintTo (this, (xe, c) => xe.Width == 170),
				this.datePicker.ConstraintTo (this, (xe, c) => xe.Height == DefaultControlHeight),
			}); */

			UpdateTheme ();
		}

		protected override void HandleErrorsChanged (object sender, System.ComponentModel.DataErrorsChangedEventArgs e)
		{
			UpdateErrorsDisplayed (ViewModel.GetErrors (ViewModel.Property.Name));
		}

		protected override void UpdateErrorsDisplayed (IEnumerable errors)
		{
			if (ViewModel.HasErrors) {
				SetErrors (errors);
			} else {
				SetErrors (null);
				SetEnabled ();
			}
		}

		protected override void SetEnabled ()
		{
			this.datePicker.Enabled = ViewModel.Property.CanWrite;
		}

		protected override void UpdateAccessibilityValues ()
		{
			this.datePicker.AccessibilityEnabled = this.datePicker.Enabled;
			// TODO With Correct Localization - this.datePicker.AccessibilityTitle = string.Format (LocalizationResources.AccessibilityXEditor, ViewModel.Property.Name);
		}

		protected override void UpdateValue ()
		{
			this.datePicker.DateValue = NSDate.FromTimeIntervalSinceReferenceDate (ViewModel.Value.Ticks); // From* may need tweaking.
		}

		protected void OnInputUpdated (object sender, EventArgs e)
		{
			NSDateComponents dateComponents = NSCalendar.CurrentCalendar.Components (NSCalendarUnit.Year | NSCalendarUnit.Month | NSCalendarUnit.Day | NSCalendarUnit.Hour | NSCalendarUnit.Minute | NSCalendarUnit.Second, this.datePicker.DateValue);
			ViewModel.Value = new CommonDateTime ((int)dateComponents.Year, (int)dateComponents.Month, (int)dateComponents.Day, (int)dateComponents.Hour, (int)dateComponents.Minute, (int)dateComponents.Second);
		}
	}
}
