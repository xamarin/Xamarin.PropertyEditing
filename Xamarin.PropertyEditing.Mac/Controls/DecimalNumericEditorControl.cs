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
			NumericEditor.Activated += (sender, e) => {
				ViewModel.Value = NumericEditor.DoubleValue;
			};

			Stepper.Activated += (s, e) => {
				ViewModel.Value = Stepper.DoubleValue;
			};
		}

		internal new FloatingPropertyViewModel ViewModel {
			get { return (FloatingPropertyViewModel)base.ViewModel; }
			set { base.ViewModel = value; }
		}

		protected override void HandlePropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof (FloatingPropertyViewModel.Value)) {
				UpdateModelValue ();
			}
		}

		protected override void UpdateModelValue ()
		{
			Stepper.DoubleValue = ViewModel.Value;
			NumericEditor.DoubleValue = ViewModel.Value;
		}
	}
}
