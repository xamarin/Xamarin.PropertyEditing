using System;
using AppKit;
using Foundation;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class IntegerNumericEditorControl : BaseNumericEditorControl
	{
		public IntegerNumericEditorControl ()
		{
			NumberStyle = NSNumberFormatterStyle.None;

			// update the VM value
			NumericEditor.Activated += (sender, e) => {
				ViewModel.Value = NumericEditor.NIntValue;
			};

			Stepper.Activated += (s, e) => {
				ViewModel.Value = Stepper.NIntValue;
			};
		}

		internal new IntegerPropertyViewModel ViewModel {
			get { return (IntegerPropertyViewModel)base.ViewModel; }
			set { base.ViewModel = value; }
		}

		protected override void UpdateValue ()
		{
			Stepper.NIntValue = (nint)ViewModel.Value;
			NumericEditor.NIntValue = (nint)ViewModel.Value;
		}
	}
}
