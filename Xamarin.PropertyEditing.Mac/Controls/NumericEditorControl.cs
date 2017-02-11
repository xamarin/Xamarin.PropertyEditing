using System;
using AppKit;
using CoreGraphics;
using Foundation;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal abstract class BaseNumericEditorControl : PropertyEditorControl
	{
		protected NSLayoutConstraint rightSideConstraint;

		public BaseNumericEditorControl ()
		{
			base.TranslatesAutoresizingMaskIntoConstraints = false;

			var controlSize = NSControlSize.Small;

			NumericEditor = new NSTextField ();
			NumericEditor.TranslatesAutoresizingMaskIntoConstraints = false;
			NumericEditor.BackgroundColor = NSColor.Clear;
			NumericEditor.DoubleValue = 0.0;
			NumericEditor.Alignment = NSTextAlignment.Right;

			formatter.FormatterBehavior = NSNumberFormatterBehavior.Version_10_4;
			formatter.Locale = NSLocale.CurrentLocale;

			NumericEditor.Cell.Formatter = formatter;
			NumericEditor.Cell.ControlSize = controlSize;



			Stepper = new NSStepper ();
			Stepper.TranslatesAutoresizingMaskIntoConstraints = false;
			Stepper.ValueWraps = false;
			Stepper.Cell.ControlSize = controlSize;


			AddSubview (Stepper);
			AddSubview (NumericEditor);

			AddConstraints (new NSLayoutConstraint[] {
				NSLayoutConstraint.Create (NumericEditor, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this, NSLayoutAttribute.Left, 1, 0),
				(rightSideConstraint = NSLayoutConstraint.Create (Stepper, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this, NSLayoutAttribute.Right, 1, 0)),
				NSLayoutConstraint.Create (NumericEditor, NSLayoutAttribute.Right, NSLayoutRelation.Equal, Stepper, NSLayoutAttribute.Left, 1, -3),
				NSLayoutConstraint.Create (NumericEditor, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this, NSLayoutAttribute.Top, 1, 0),
				NSLayoutConstraint.Create (NumericEditor, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this, NSLayoutAttribute.Top, 1, 0)
			});
		}

		internal NSTextField NumericEditor { get; set; }
		internal NSStepper Stepper { get; set; }

		NSNumberFormatter formatter = new NSNumberFormatter ();
		NSNumberFormatterStyle numberStyle;
		protected NSNumberFormatterStyle NumberStyle {
			get { return numberStyle; }
			set { 
				numberStyle = value; 
				formatter.NumberStyle = numberStyle;
			}
		}

		protected override abstract void HandlePropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e);

		protected override abstract void UpdateModelValue ();
	}

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

		protected override void HandlePropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof (IntegerPropertyViewModel.Value)) {
				UpdateModelValue ();
			}
		}

		protected override void UpdateModelValue ()
		{
			Stepper.NIntValue = (nint)ViewModel.Value;
			NumericEditor.NIntValue = (nint)ViewModel.Value;
		}
	}

	internal class DecimalNumericEditorControl : BaseNumericEditorControl
	{
		public DecimalNumericEditorControl () 
		{
			NumberStyle = NSNumberFormatterStyle.Decimal;

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
