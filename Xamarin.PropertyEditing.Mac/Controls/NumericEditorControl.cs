using System;
using System.Collections;
using AppKit;
using Foundation;
using Xamarin.PropertyEditing.Mac.Resources;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class NumericEditorControl<T>
		: PropertyEditorControl
		where T : struct
	{
		public NumericEditorControl ()
		{
			base.TranslatesAutoresizingMaskIntoConstraints = false;

			NumericEditor = new NumericSpinEditor ();
			NumericEditor.ValueChanged += OnValueChanged;

			TypeCode code = Type.GetTypeCode (typeof (T));
			switch (code) {
				case TypeCode.Double:
				case TypeCode.Single:
				case TypeCode.Decimal:
					NumberStyle = NSNumberFormatterStyle.Decimal;
					Formatter.UsesGroupingSeparator = false;
					Formatter.MaximumFractionDigits = 15;
					break;
				default:
					NumberStyle = NSNumberFormatterStyle.None;
					break;
			}

			AddSubview (NumericEditor);

			this.DoConstraints ( new[] {
				NumericEditor.ConstraintTo (this, (n, c) => n.Top == c.Top + 1),
				NumericEditor.ConstraintTo (this, (n, c) => n.Left == c.Left + 4),
				NumericEditor.ConstraintTo (this, (n, c) => n.Width == c.Width - 33),
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
				SetErrors (errors);
			} else {
				SetErrors (null);
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

		protected virtual void OnValueChanged (object sender, EventArgs e)
		{
			((PropertyViewModel<T>)ViewModel).Value = (T)Convert.ChangeType (NumericEditor.Value, typeof(T));
		}

		protected override void UpdateValue()
		{
			NumericEditor.Value = (double)Convert.ChangeType (((PropertyViewModel<T>)ViewModel).Value, typeof(double));
		}

		protected override void UpdateAccessibilityValues ()
		{
			NumericEditor.AccessibilityEnabled = NumericEditor.Enabled;
			NumericEditor.AccessibilityTitle = string.Format (LocalizationResources.AccessibilityNumeric, ViewModel.Property.Name);
		}
	}
}
