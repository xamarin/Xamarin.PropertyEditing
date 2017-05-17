using System;
using Foundation;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class DecimalNumericEditorControl : BaseNumericEditorControl
	{
		public DecimalNumericEditorControl ()
		{
			NumberStyle = NSNumberFormatterStyle.Decimal;
			Formatter.UsesGroupingSeparator = false;
			Formatter.MaximumFractionDigits = 15;

			// update the VM value
			NumericEditor.ValueChanged += (sender, e) => {
				ViewModel.Value = NumericEditor.Value;
			};
		}

		internal new FloatingPropertyViewModel ViewModel {
			get { return (FloatingPropertyViewModel)base.ViewModel; }
			set { base.ViewModel = value; }
		}

		protected override void UpdateValue ()
		{
			NumericEditor.Value = ViewModel.Value;
		}
	}
}
