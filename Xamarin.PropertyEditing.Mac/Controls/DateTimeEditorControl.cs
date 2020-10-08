using System;

namespace Xamarin.PropertyEditing.Mac
{
	internal class DateTimeEditorControl : BaseDateTimeEditorControl<DateTime>
	{
		public DateTimeEditorControl (IHostResourceProvider hostResources)
			: base (hostResources)
		{
		}

		protected override void Editor_Activated (object sender, EventArgs e)
		{
			ViewModel.Value = DatePicker.DateValue.ToDateTime ();
		}

		protected override void UpdateValue ()
		{
			DatePicker.DateValue = ViewModel.Value.ToNSDate ();
		}
	}
}
