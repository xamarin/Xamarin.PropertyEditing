using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AppKit;
using Foundation;
using Xamarin.PropertyEditing.Mac.Resources;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class NumericEditorControl<T>
		: PropertyEditorControl<NumericPropertyViewModel<T>>
	{
		public NumericEditorControl (IHostResourceProvider hostResources)
			: base (hostResources)
		{
			base.TranslatesAutoresizingMaskIntoConstraints = false;

			NumericEditor = new NumericSpinEditor<T> (hostResources);
			NumericEditor.ValueChanged += OnValueChanged;

			var t = typeof (T);
			if (t.Name == PropertyViewModel<T>.NullableName) {
				this.underlyingType = Nullable.GetUnderlyingType (t);
				t = this.underlyingType;
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

			this.numericEditorWidthConstraint = NSLayoutConstraint.Create (NumericEditor, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this, NSLayoutAttribute.Width, 1f, -32f);

			this.AddConstraints (new[] {
				NSLayoutConstraint.Create (NumericEditor, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Top, 1f, 1f),
				this.numericEditorWidthConstraint,
			});
		}

		protected NumericSpinEditor<T> NumericEditor { get; set; }

		protected NSNumberFormatter Formatter {
			get { return NumericEditor.Formatter; }
			set { NumericEditor.Formatter = value; }
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

		internal NSPopUpButton inputModePopup;
		private IReadOnlyList<InputMode> viewModelInputModes;

		private NSLayoutConstraint numericEditorWidthConstraint;

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
			if (this.inputModePopup != null)
				this.inputModePopup.Enabled = ViewModel.Property.CanWrite;
		}

		protected virtual void OnValueChanged (object sender, EventArgs e)
		{
			var t = typeof (T);
			if (this.underlyingType != null)
				t = this.underlyingType;
			ViewModel.Value = (T)Convert.ChangeType (NumericEditor.Value, t);
		}

		protected override void UpdateValue()
		{
			if (this.underlyingType != null) {
				NumericEditor.StringValue = ViewModel.Value == null ? string.Empty : ViewModel.Value.ToString ();
				NumericEditor.Enabled = ((ViewModel.InputMode != null) && !ViewModel.InputMode.IsSingleValue) || (this.inputModePopup == null);

				if (this.inputModePopup != null)
					this.inputModePopup.SelectItem ((ViewModel.InputMode == null) ? string.Empty : ViewModel.InputMode.Identifier);
			} else {
				NumericEditor.Value = (double)Convert.ChangeType (ViewModel.Value, typeof (double));

				if (this.inputModePopup != null)
					this.inputModePopup.SelectItem (-1);
			}
		}

		protected override void UpdateAccessibilityValues ()
		{
			NumericEditor.AccessibilityEnabled = NumericEditor.Enabled;
			NumericEditor.AccessibilityTitle = string.Format (LocalizationResources.AccessibilityNumeric, ViewModel.Property.Name);

			if (this.inputModePopup != null) {
				this.inputModePopup.AccessibilityEnabled = NumericEditor.Enabled;
				this.inputModePopup.AccessibilityTitle = string.Format (LocalizationResources.AccessibilityInpueModeEditor, ViewModel.Property.Name);
			}
		}

		protected override void OnViewModelChanged (PropertyViewModel oldModel)
		{
			base.OnViewModelChanged (oldModel);

			if (ViewModel == null)
				return;

			if (ViewModel.HasInputModes) {
				if (this.inputModePopup == null) {
					this.inputModePopup = new NSPopUpButton {
						Menu = new NSMenu (),
						TranslatesAutoresizingMaskIntoConstraints = false,
					};

					this.inputModePopup.Activated += (o, e) => {
						var popupButton = o as NSPopUpButton;
						ViewModel.InputMode = this.viewModelInputModes.FirstOrDefault (im => im.Identifier == popupButton.Title);
					};

					AddSubview (this.inputModePopup);

					this.AddConstraints (new[] {
						NSLayoutConstraint.Create (this.inputModePopup, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Top, 1f, 1f),
						NSLayoutConstraint.Create (this.inputModePopup, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Right, 1f, -33f),
						NSLayoutConstraint.Create (this.inputModePopup, NSLayoutAttribute.Width, NSLayoutRelation.Equal, 1f, 80f),
						NSLayoutConstraint.Create (this.inputModePopup, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, DefaultControlHeight - 3 ),
					});
				}

				this.inputModePopup.Menu.RemoveAllItems ();
				this.viewModelInputModes = ViewModel.InputModes;
				foreach (InputMode item in this.viewModelInputModes) {
					this.inputModePopup.Menu.AddItem (new NSMenuItem (item.Identifier));
				}

				this.numericEditorWidthConstraint.Constant = -117f; // Shorten the stringEditor if we have Inputmodes Showing.
			} else {
				this.numericEditorWidthConstraint.Constant = -34f; // Lengthen the stringEditor if we have Inputmodes Hidden.
			}

			// If we are reusing the control we'll have to hid the inputMode if this doesn't have InputMode.
			if (this.inputModePopup != null)
				this.inputModePopup.Hidden = !ViewModel.HasInputModes;
		}
	}
}
