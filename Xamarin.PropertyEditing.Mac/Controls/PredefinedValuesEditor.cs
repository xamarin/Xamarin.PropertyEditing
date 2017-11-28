using System;
using System.Collections;
using System.Diagnostics;
using System.ComponentModel;

using Foundation;
using AppKit;
using CoreGraphics;

using Xamarin.PropertyEditing.ViewModels;
using Xamarin.PropertyEditing.Mac.Resources;

namespace Xamarin.PropertyEditing.Mac
{
	internal class PredefinedValuesEditor<T>
		: PropertyEditorControl
	{
		public PredefinedValuesEditor ()
		{
			base.TranslatesAutoresizingMaskIntoConstraints = false;

			this.comboBox = new NSComboBox {
				TranslatesAutoresizingMaskIntoConstraints = false,
				BackgroundColor = NSColor.Clear,
				StringValue = String.Empty,
				Cell = {
					ControlSize = NSControlSize.Small
				},
				Editable = false,
			};

			this.popUpButton = new NSPopUpButton {
				TranslatesAutoresizingMaskIntoConstraints = false,
				StringValue = String.Empty,
				Cell = {
					ControlSize = NSControlSize.Small
				},
			};

			this.comboBox.SelectionChanged += (sender, e) => {
				EditorViewModel.ValueName = comboBox.SelectedValue.ToString ();
			};

			popupButtonList = new NSMenu ();
			popUpButton.Menu = popupButtonList;

			popUpButton.Activated += (o, e) => {
				EditorViewModel.ValueName = (o as NSPopUpButton).Title;
			};

			UpdateTheme ();
		}

		public override NSView FirstKeyView => firstKeyView;
		public override NSView LastKeyView => lastKeyView;

		protected PredefinedValuesViewModel<T> EditorViewModel => (PredefinedValuesViewModel<T>)ViewModel;

		readonly NSComboBox comboBox;
		readonly NSPopUpButton popUpButton;
		NSMenu popupButtonList;

		bool dataPopulated;
		NSView firstKeyView;
		NSView lastKeyView;

		protected override void HandleErrorsChanged (object sender, DataErrorsChangedEventArgs e)
		{
			UpdateErrorsDisplayed (ViewModel.GetErrors (e.PropertyName));
		}

		protected override void SetEnabled ()
		{
			if (EditorViewModel.IsConstrainedToPredefined) {
				this.popUpButton.Enabled = ViewModel.Property.CanWrite;
			} else {
				this.comboBox.Enabled = ViewModel.Property.CanWrite;
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

		protected override void OnViewModelChanged (PropertyViewModel oldModel)
		{
			if (!dataPopulated) {
				if (EditorViewModel.IsConstrainedToPredefined) {
					this.popupButtonList.RemoveAllItems ();
					foreach (string item in EditorViewModel.PossibleValues) {
						popupButtonList.AddItem (new NSMenuItem (item));
					}

					AddSubview (this.popUpButton);

					this.DoConstraints (new[] {
						popUpButton.ConstraintTo (this, (pub, c) => pub.Width == c.Width - 26),
						popUpButton.ConstraintTo (this, (pub, c) => pub.Left == c.Left + 3),
						popUpButton.ConstraintTo (this, (pub, c) => pub.Top == c.Top + 6),
					});

					firstKeyView = this.popUpButton;
					lastKeyView = this.popUpButton;
				} else {
					this.comboBox.RemoveAll ();

					// Once the VM is loaded we need a one time population
					foreach (var item in EditorViewModel.PossibleValues) {
						this.comboBox.Add (new NSString (item));
					}

					AddSubview (this.comboBox);

					this.DoConstraints (new[] {
						comboBox.ConstraintTo (this, (cb, c) => cb.Width == c.Width - 28),
						comboBox.ConstraintTo (this, (cb, c) => cb.Left == c.Left + 3),
						comboBox.ConstraintTo (this, (cb, c) => cb.Top == c.Top + 4),
					});

					firstKeyView = this.comboBox;
					lastKeyView = this.comboBox;
				}

				dataPopulated = true;
			}

			base.OnViewModelChanged (oldModel);
		}

		protected override void UpdateValue ()
		{
			if (EditorViewModel.IsConstrainedToPredefined) {
				this.popUpButton.Title = EditorViewModel.ValueName ?? String.Empty;
			} else {
				this.comboBox.StringValue = EditorViewModel.ValueName ?? String.Empty;
			}
		}

		protected override void UpdateAccessibilityValues ()
		{
			if (EditorViewModel.IsConstrainedToPredefined) {
				popUpButton.AccessibilityEnabled = popUpButton.Enabled;
				popUpButton.AccessibilityTitle = string.Format (LocalizationResources.AccessibilityCombobox, ViewModel.Property.Name);
			} else {
				comboBox.AccessibilityEnabled = comboBox.Enabled;
				comboBox.AccessibilityTitle = string.Format (LocalizationResources.AccessibilityCombobox, ViewModel.Property.Name);
			}
		}
	}
}
