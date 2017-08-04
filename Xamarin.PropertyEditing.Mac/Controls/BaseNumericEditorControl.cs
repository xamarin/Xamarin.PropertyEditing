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
		//protected NSLayoutConstraint rightSideConstraint;

		public BaseNumericEditorControl ()
		{
			base.TranslatesAutoresizingMaskIntoConstraints = false;

			var controlSize = NSControlSize.Small;

			NumericEditor = new NSTextField {
				TranslatesAutoresizingMaskIntoConstraints = false,
				DoubleValue = 0.0,
				Alignment = NSTextAlignment.Right,
				Editable = false,
			};

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

			this.DoConstraints ( new[] {
				NumericEditor.ConstraintTo (this, (n, c) => n.Width == c.Width - 17),
				Stepper.ConstraintTo (NumericEditor, (s, n) => s.Left == n.Right + 5),
				Stepper.ConstraintTo (NumericEditor, (s, n) => s.Top == n.Top + 1),
			});
		}

		internal NSTextField NumericEditor { get; set; }
		internal NSStepper Stepper { get; set; }

		public override NSView FirstKeyView => NumericEditor;
		public override NSView LastKeyView => NumericEditor;

		protected NSNumberFormatter Formatter = new NSNumberFormatter ();
		NSNumberFormatterStyle numberStyle;
		protected NSNumberFormatterStyle NumberStyle {
			get { return numberStyle; }
			set {
				numberStyle = value;
				Formatter.NumberStyle = numberStyle;
			}
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
				NumericEditor.BackgroundColor = NSColor.Clear;
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
		}

		protected override void UpdateAccessibilityValues ()
		{
			NumericEditor.AccessibilityEnabled = NumericEditor.Enabled;
			NumericEditor.AccessibilityTitle = ViewModel.Property.Name + " Numeric Editor"; // TODO Localization

			Stepper.AccessibilityEnabled = Stepper.Enabled;
			Stepper.AccessibilityTitle = ViewModel.Property.Name + " Numeric Stepper"; // TODO Localization
		}
	}
}
