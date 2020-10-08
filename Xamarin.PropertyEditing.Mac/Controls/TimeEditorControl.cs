using System;
using AppKit;
using Xamarin.PropertyEditing.Common;

namespace Xamarin.PropertyEditing.Mac
{
	internal class TimeEditorControl : BaseDateTimeEditorControl<Time>
	{
		public TimeEditorControl (IHostResourceProvider hostResources)
			: base (hostResources)
		{
			DatePicker.DatePickerElements = NSDatePickerElementFlags.HourMinuteSecond;
		}

		protected override void Editor_Activated (object sender, EventArgs e)
		{
			ViewModel.Value = new Time (DatePicker.DateValue.ToDateTime ());
		}


		protected override void UpdateValue ()
		{
			if (ViewModel.Value != null)
				DatePicker.DateValue = ViewModel.Value.DateTime.ToNSDate ();
		}
	}
}
