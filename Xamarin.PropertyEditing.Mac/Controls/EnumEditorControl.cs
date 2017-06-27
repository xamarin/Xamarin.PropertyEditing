using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using AppKit;
using CoreGraphics;
using Foundation;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class EnumEditorControl<T> : PropertyEditorControl
		where T : struct
	{
		NSComboBox ComboBoxEditor;
		List<NSButton> FlagsList = new List<NSButton> ();

		public override NSView FirstKeyView => ComboBoxEditor;
		public override NSView LastKeyView => ComboBoxEditor;

		EnumPropertyViewModel<T> EnumEditorViewModel => (EnumPropertyViewModel<T>)ViewModel;

		bool dataPopulated;

		public EnumEditorControl ()
		{
			base.TranslatesAutoresizingMaskIntoConstraints = false;

			ComboBoxEditor = new NSComboBox (new CGRect (0, 0, 150, 20));
			ComboBoxEditor.TranslatesAutoresizingMaskIntoConstraints = false;
			ComboBoxEditor.BackgroundColor = NSColor.Clear;
			ComboBoxEditor.StringValue = string.Empty;
			ComboBoxEditor.Cell.ControlSize = NSControlSize.Regular;

			ComboBoxEditor.SelectionChanged += (sender, e) => {
				EnumEditorViewModel.ValueName = ComboBoxEditor.SelectedValue.ToString ();
			};
		}


		protected override void HandleErrorsChanged (object sender, DataErrorsChangedEventArgs e)
		{
			UpdateErrorsDisplayed (ViewModel.GetErrors (ViewModel.Property.Name));
		}

		protected override void HandlePropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			Debug.WriteLine ("Proerty Name : {0}", e.PropertyName);
			if (e.PropertyName == nameof (EnumEditorViewModel.ValueName)) {
				UpdateModelValue ();
			}
		}

		protected override void SetEnabled ()
		{
			ComboBoxEditor.Editable = ViewModel.Property.CanWrite;
		}

		protected override void UpdateErrorsDisplayed (IEnumerable errors)
		{
			if (ViewModel.HasErrors) {
				ComboBoxEditor.BackgroundColor = NSColor.Red;
				Debug.WriteLine ("Your input triggered an error:");
				foreach (var error in errors) {
					Debug.WriteLine (error.ToString () + "\n");
				}
			}
			else {
				ComboBoxEditor.BackgroundColor = NSColor.Clear;
				SetEnabled ();
			}
		}

		protected override void UpdateModelValue ()
		{
			if (!dataPopulated) {
				if (EnumEditorViewModel.IsFlags) {
					var top = 0;
					foreach (var item in EnumEditorViewModel.PossibleValues) {
						var BooleanEditor = new NSButton (new CGRect(0, top, 200, 24)) { TranslatesAutoresizingMaskIntoConstraints = false };
						BooleanEditor.SetButtonType (NSButtonType.Switch);
						BooleanEditor.Title = item.Key;
						BooleanEditor.State = item.Value ? NSCellStateValue.On : NSCellStateValue.Off;
						BooleanEditor.Activated += BooleanEditor_Activated;
						BooleanEditor.Enabled = false; // TODO Remove this line once EnumEditorViewModel.Value is updated correctly.
                        AddSubview (BooleanEditor);
						FlagsList.Add (BooleanEditor);
						top += 24;
					}

					EnumEditorViewModel.RowHeight = EnumEditorViewModel.PossibleValues.Count * 24;
				} else {
					// Once the VM is loaded we need a one time population
					if (ViewModel.Property.Type.IsEnum && ComboBoxEditor.Count == 0) {
						foreach (var item in EnumEditorViewModel.PossibleValues) {
							ComboBoxEditor.Add (new NSString (item.Key));
						}
					}
					AddSubview (ComboBoxEditor);

					this.DoConstraints (new[] {
					ComboBoxEditor.ConstraintTo (this, (cb, c) => cb.Width == c.Width),
				});
				}
				dataPopulated = true;
			}

			base.UpdateModelValue ();

			if (EnumEditorViewModel.IsFlags) {
				for (int i = 0; i > FlagsList.Count - 1; i++) {
					FlagsList[i].State = EnumEditorViewModel.PossibleValues[FlagsList[i].Title] ? NSCellStateValue.On : NSCellStateValue.Off;
				}
			} else {
				ComboBoxEditor.StringValue = EnumEditorViewModel.ValueName;
			}
		}

		void BooleanEditor_Activated (object sender, EventArgs e)
		{
			var btn = sender as NSButton;
			T realValue;
			if (Enum.TryParse<T> (btn.Title, out realValue)) {
				// TODO we need an elegant way for EnumEditorViewModel.Value |= realValue; to work
			}

		}
	}
}