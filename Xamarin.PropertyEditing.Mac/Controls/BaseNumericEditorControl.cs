using System;
using System.Collections;
using System.Diagnostics;
using AppKit;
using CoreGraphics;
using Foundation;
using Xamarin.PropertyEditing.Mac.Resources;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal abstract class BaseNumericEditorControl : PropertyEditorControl
	{
		public BaseNumericEditorControl ()
		{
			base.TranslatesAutoresizingMaskIntoConstraints = false;

			NumericEditor = new NumericSpinEditor ();
			AddSubview (NumericEditor);

			this.DoConstraints ( new[] {
				NumericEditor.ConstraintTo (this, (n, c) => n.Width == c.Width),
			});
		}

		protected NumericSpinEditor NumericEditor { get; set; }

		protected NSNumberFormatter Formatter {
			get {
				return NumericEditor.Formatter;
			}
			set {
				NumericEditor.Formatter = value;
			}
		}

		public override NSView FirstKeyView => NumericEditor;
		public override NSView LastKeyView => NumericEditor;

		protected NSNumberFormatterStyle NumberStyle {
			get { 
				return NumericEditor.NumberStyle; }
			set {
				NumericEditor.NumberStyle = value;
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
		}

		protected override void UpdateAccessibilityValues ()
		{
			NumericEditor.AccessibilityEnabled = NumericEditor.Enabled;
			NumericEditor.AccessibilityTitle = Strings.AccessibilityNumeric (ViewModel.Property.Name);
		}
	}
}
