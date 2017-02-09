using System;
using AppKit;
using CoreGraphics;
using Foundation;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class BaseNumericEditorControl : PropertyEditorControl
	{
		public BaseNumericEditorControl ()
		{
			NumericEditor = new NSTextField (new CGRect (0, 0, 150, 20));
			NumericEditor.BackgroundColor = NSColor.Clear;
			NumericEditor.DoubleValue = 0.0;

			formatter.FormatterBehavior = NSNumberFormatterBehavior.Version_10_4;
			formatter.Locale = NSLocale.CurrentLocale;

			NumericEditor.Cell.Formatter = formatter;

			// update the value on 'enter'
			NumericEditor.Activated += (sender, e) => {
				ViewModel.Value = NumericEditor.DoubleValue;
			};
			AddSubview (NumericEditor);
		}

		protected NSTextField NumericEditor { get; set; }

		NSNumberFormatter formatter = new NSNumberFormatter ();
		NSNumberFormatterStyle numberStyle;
		protected NSNumberFormatterStyle NumberStyle {
			get { return numberStyle; }
			set { 
				numberStyle = value; 
				formatter.NumberStyle = numberStyle;
			}
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
			NumericEditor.DoubleValue = ViewModel.Value;
		}
	}

	internal class IntegerNumericEditorControl : BaseNumericEditorControl
	{
		public IntegerNumericEditorControl ()
		{
			NumberStyle = NSNumberFormatterStyle.None;
		}
	}

	internal class DecimalNumericEditorControl : BaseNumericEditorControl
	{
		public DecimalNumericEditorControl () 
		{
			NumberStyle = NSNumberFormatterStyle.Decimal;
		}
	}

	internal class CurrencyNumericEditorControl : BaseNumericEditorControl
	{
		public CurrencyNumericEditorControl ()
		{
			NumberStyle = NSNumberFormatterStyle.Currency;
		}
	}
}
