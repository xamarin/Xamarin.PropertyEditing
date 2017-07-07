using System;
using System.Collections;
using System.Diagnostics;
using System.ComponentModel;

using Foundation;
using AppKit;
using CoreGraphics;

using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class PredefinedValuesEditor<T>
		: PropertyEditorControl
	{
		public PredefinedValuesEditor ()
		{
			base.TranslatesAutoresizingMaskIntoConstraints = false;

			this.comboBox = new NSComboBox (new CGRect (0, 0, 150, 20)) {
				TranslatesAutoresizingMaskIntoConstraints = false,
				BackgroundColor = NSColor.Clear,
				StringValue = String.Empty,
				Cell = {
					ControlSize = NSControlSize.Regular	
				}
			};

			this.comboBox.SelectionChanged += (sender, e) => {
				EditorViewModel.ValueName = comboBox.SelectedValue.ToString ();
			};

			AddSubview (this.comboBox);

			this.DoConstraints (new[] {
				comboBox.ConstraintTo (this, (cb, c) => cb.Width == c.Width),
			});
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
				this.comboBox.BackgroundColor = NSColor.Red;
				Debug.WriteLine ("Your input triggered an error:");
				foreach (var error in errors) {
					Debug.WriteLine (error.ToString () + "\n");
				}
			} else {
				comboBox.BackgroundColor = NSColor.Clear;
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
	}
}
