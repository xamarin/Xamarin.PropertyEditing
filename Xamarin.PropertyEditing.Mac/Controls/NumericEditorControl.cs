using System;
using System.Collections;
using AppKit;
using Foundation;
using Xamarin.PropertyEditing.Mac.Resources;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class NumericEditorControl<T>
		: PropertyEditorControl<NumericPropertyViewModel<T>>
	{
		public NumericEditorControl ()
		{
			base.TranslatesAutoresizingMaskIntoConstraints = false;

			NumericEditor = new NumericSpinEditor<T> ();
			NumericEditor.ValueChanged += OnValueChanged;

			var t = typeof (T);
			if (t.Name == PropertyViewModel<T>.NullableName) {
				underlyingType = Nullable.GetUnderlyingType (t);
				t = underlyingType;
			}
			TypeCode code = Type.GetTypeCode (t);
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

			this.AddConstraints (new[] {
				NSLayoutConstraint.Create (NumericEditor, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Top, 1f, 1f),
				NSLayoutConstraint.Create (NumericEditor, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Width, 1f, -32f),
			});
		}

		protected NumericSpinEditor<T> NumericEditor { get; set; }

		protected NSNumberFormatter Formatter {
			get {
				return NumericEditor.Formatter;
			}
			set {
				NumericEditor.Formatter = value;
			}
		}

		public override NSView FirstKeyView => NumericEditor;
		public override NSView LastKeyView => NumericEditor.DecrementButton;

		protected NSNumberFormatterStyle NumberStyle {
			get { 
				return NumericEditor.NumberStyle; }
			set {
				NumericEditor.NumberStyle = value;
			}
		}

		private Type underlyingType;

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
			var t = typeof (T);
			if (underlyingType != null)
				t = underlyingType;
			ViewModel.Value = (T)Convert.ChangeType (NumericEditor.Value, t);
		}

		protected override void UpdateValue()
		{
			if (underlyingType != null) {
				NumericEditor.StringValue = ViewModel.Value == null ? string.Empty : ViewModel.Value.ToString ();
			} else {
				NumericEditor.Value = (double)Convert.ChangeType (ViewModel.Value, typeof (double));
			}
		}

		protected override void UpdateAccessibilityValues ()
		{
			NumericEditor.AccessibilityEnabled = NumericEditor.Enabled;
			NumericEditor.AccessibilityTitle = string.Format (LocalizationResources.AccessibilityNumeric, ViewModel.Property.Name);
		}
	}
}
