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

			this.comboBox = new NSComboBox () {
				TranslatesAutoresizingMaskIntoConstraints = false,
				BackgroundColor = NSColor.Clear,
				StringValue = String.Empty,
				Cell = {
					ControlSize = NSControlSize.Regular
				},
				Editable = false,
			};

			this.comboBox.SelectionChanged += (sender, e) => {
				EditorViewModel.ValueName = comboBox.SelectedValue.ToString ();
			};

			AddSubview (this.comboBox);

			this.DoConstraints (new[] {
				comboBox.ConstraintTo (this, (cb, c) => cb.Width == c.Width - 28),
				comboBox.ConstraintTo (this, (cb, c) => cb.Left == c.Left + 3),
			});

			UpdateTheme ();
		}

		public override NSView FirstKeyView => this.comboBox;
		public override NSView LastKeyView => this.comboBox;

		protected PredefinedValuesViewModel<T> EditorViewModel => (PredefinedValuesViewModel<T>)ViewModel;

		protected override void HandleErrorsChanged (object sender, DataErrorsChangedEventArgs e)
		{
			UpdateErrorsDisplayed (ViewModel.GetErrors (e.PropertyName));
		}

		protected override void SetEnabled ()
		{
			this.comboBox.Editable = ViewModel.Property.CanWrite;
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
			this.comboBox.RemoveAll ();
			foreach (string item in EditorViewModel.PossibleValues) {
				this.comboBox.Add (new NSString (item));
			}

			base.OnViewModelChanged (oldModel);
		}

		protected override void UpdateValue ()
		{
			this.comboBox.StringValue = EditorViewModel.ValueName ?? String.Empty;
		}

		private readonly NSComboBox comboBox;

		protected override void UpdateAccessibilityValues ()
		{
			comboBox.AccessibilityEnabled = comboBox.Enabled;
			comboBox.AccessibilityTitle = Strings.AccessibilityCombobox (ViewModel.Property.Name);
		}
	}
}
