using System;
namespace Xamarin.PropertyEditing.Mac.Controls.Custom.Focusable
{
	internal interface IFocusable
	{
		event EventHandler GainedFocus;

		void TriggerFocus (object sender);
	}
}
