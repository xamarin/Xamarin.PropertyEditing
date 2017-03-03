using System;
using System.Collections;
using System.Diagnostics;
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
			SetEnabled ();
			NumericEditor.DoubleValue = 0.0;
			NumericEditor.Alignment = NSTextAlignment.Right;

			Formatter.FormatterBehavior = NSNumberFormatterBehavior.Version_10_4;
			Formatter.Locale = NSLocale.CurrentLocale;

			NumericEditor.Cell.Formatter = Formatter;
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

		protected NSNumberFormatter Formatter = new NSNumberFormatter ();
		NSNumberFormatterStyle numberStyle;
		protected NSNumberFormatterStyle NumberStyle {
			get { return numberStyle; }
			set {
				numberStyle = value;
				Formatter.NumberStyle = numberStyle;
			}
		}

		protected override abstract void HandlePropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e);

		protected override void UpdateModelValue ()
		{
			base.UpdateModelValue ();
		}

		protected override void UpdateErrorsDisplayed (IEnumerable errors)
		{
			if (ViewModel.HasErrors) {
				NumericEditor.BackgroundColor = NSColor.Red;
				Debug.WriteLine ("Your input triggered an error:");
				foreach (var error in errors) {
					Debug.WriteLine (error.ToString () + "\n");
				}
			}
			else {
				SetEnabled ();
			}
		}

		protected override void HandleErrorsChanged (object sender, System.ComponentModel.DataErrorsChangedEventArgs e)
		{
			UpdateErrorsDisplayed (ViewModel.GetErrors (ViewModel.Property.Name));
		}

		protected override void SetEnabled ()
		{
			NumericEditor.Editable = ViewModel.Property.CanWrite;
			Stepper.Enabled = ViewModel.Property.CanWrite;
			if (NumericEditor.Editable) {
				NumericEditor.BackgroundColor = NSColor.Clear;
			}
		}
	}
}
