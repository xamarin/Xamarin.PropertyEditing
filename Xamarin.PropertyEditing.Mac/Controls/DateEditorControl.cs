using System;
using AppKit;
using Xamarin.PropertyEditing.Common;

namespace Xamarin.PropertyEditing.Mac
{
	internal class DateEditorControl : BaseDateTimeEditorControl<Date>
	{

		public DateEditorControl (IHostResourceProvider hostResources)
			: base (hostResources)
		{
			DatePicker.DatePickerElements = NSDatePickerElementFlags.YearMonthDateDay;
		}

		protected override void Editor_Activated (object sender, EventArgs e)
		{
			ViewModel.Value = new Date (DatePicker.DateValue.ToDateTime ());
		}

		protected override void UpdateValue ()
		{
			if (ViewModel.Value != null)
				DatePicker.DateValue = ViewModel.Value.DateTime.ToNSDate ();
		}
	}
}
