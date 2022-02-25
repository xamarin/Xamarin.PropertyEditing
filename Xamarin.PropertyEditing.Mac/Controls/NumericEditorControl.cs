using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AppKit;
using Foundation;
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

			NumericEditor = new NumericSpinEditor<T> (hostResources)
			{
				ProxyResponder = new ProxyResponder (this, ProxyRowType.SingleView)
			};
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

			this.editorRightConstraint = NSLayoutConstraint.Create (NumericEditor, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this, NSLayoutAttribute.Right, 1f, 0);

			this.AddConstraints (new[] {
				NSLayoutConstraint.Create (NumericEditor, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, this,  NSLayoutAttribute.CenterY, 1f, 0),
				NSLayoutConstraint.Create (NumericEditor, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this, NSLayoutAttribute.Left, 1, 0),
				this.editorRightConstraint,
				NSLayoutConstraint.Create (NumericEditor, NSLayoutAttribute.Height, NSLayoutRelation.Equal, this, NSLayoutAttribute.Height, 1, -6),
			});
		}

		protected NumericSpinEditor<T> NumericEditor { get; set; }

		protected NSNumberFormatter Formatter {
			get { return NumericEditor.Formatter; }
			set { NumericEditor.Formatter = value; }
		}

		public override NSView FirstKeyView => NumericEditor;
		public override NSView LastKeyView => NumericEditor.DecrementButton;

		private bool CanEnable => ViewModel.Property.CanWrite && (((ViewModel.InputMode != null) && !ViewModel.InputMode.IsSingleValue) || (this.inputModePopup == null));

		protected NSNumberFormatterStyle NumberStyle {
			get { 
				return NumericEditor.NumberStyle; }
			set {
				NumericEditor.NumberStyle = value;
			}
		}

		protected override void SetEnabled ()
		{
			NumericEditor.Enabled = CanEnable;
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
				NumericEditor.Enabled = CanEnable;

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
			NumericEditor.AccessibilityTitle = string.Format (Properties.Resources.AccessibilityNumeric, ViewModel.Property.Name);

			if (this.inputModePopup != null) {
				this.inputModePopup.AccessibilityEnabled = this.inputModePopup.Enabled;
				this.inputModePopup.AccessibilityTitle = string.Format (Properties.Resources.AccessibilityInputModeEditor, ViewModel.Property.Name);
			}
		}

		protected override void OnViewModelChanged (PropertyViewModel oldModel)
		{
			base.OnViewModelChanged (oldModel);

			if (ViewModel == null)
				return;

			this.editorRightConstraint.Active = !ViewModel.HasInputModes;
			if (ViewModel.HasInputModes) {
				if (this.inputModePopup == null) {
					this.inputModePopup = new FocusablePopUpButton {
						ControlSize = NSControlSize.Small,
						Font = NSFont.SystemFontOfSize (NSFont.SystemFontSizeForControlSize (NSControlSize.Small)),
						Menu = new NSMenu (),
						TranslatesAutoresizingMaskIntoConstraints = false,
					};

					this.NumericEditor.ProxyResponder = new ProxyResponder (this, ProxyRowType.FirstView);
					this.inputModePopup.ProxyResponder = new ProxyResponder (this, ProxyRowType.LastView);

					this.inputModePopup.Activated += (o, e) => {
						var popupButton = o as NSPopUpButton;
						ViewModel.InputMode = this.viewModelInputModes.FirstOrDefault (im => im.Identifier == popupButton.Title);
					};

					AddSubview (this.inputModePopup);
					this.editorInputModeConstraint = NSLayoutConstraint.Create (NumericEditor, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this.inputModePopup, NSLayoutAttribute.Left, 1, -4);
					AddConstraints (new[] {
						this.editorInputModeConstraint,
						NSLayoutConstraint.Create (this.inputModePopup, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, this,  NSLayoutAttribute.CenterY, 1f, 0f),
						NSLayoutConstraint.Create (this.inputModePopup, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Right, 1f, 0f),
						NSLayoutConstraint.Create (this.inputModePopup, NSLayoutAttribute.Width, NSLayoutRelation.Equal, 1f, DefaultButtonWidth),
					});
				}

				this.inputModePopup.Menu.RemoveAllItems ();
				this.viewModelInputModes = ViewModel.InputModes;
				foreach (InputMode item in this.viewModelInputModes) {
					this.inputModePopup.Menu.AddItem (new NSMenuItem (item.Identifier));
				}
			}

			// If we are reusing the control we'll have to hid the inputMode if this doesn't have InputMode.
			if (this.inputModePopup != null) {
				this.inputModePopup.Hidden = !ViewModel.HasInputModes;
				this.editorInputModeConstraint.Active = ViewModel.HasInputModes;
				UpdateAccessibilityValues ();
			}

			SetEnabled ();
		}

		private Type underlyingType;

		internal FocusablePopUpButton inputModePopup;
		private IReadOnlyList<InputMode> viewModelInputModes;

		private readonly NSLayoutConstraint editorRightConstraint;
		private NSLayoutConstraint editorInputModeConstraint;
	}
}

