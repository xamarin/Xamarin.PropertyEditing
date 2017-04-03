using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using AppKit;
using CoreGraphics;
using Foundation;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class EnumEditorControl : PropertyEditorControl
	{
		NSComboBox ComboBoxEditor;

		public EnumEditorControl ()
		{
			base.TranslatesAutoresizingMaskIntoConstraints = false;

			ComboBoxEditor = new NSComboBox (new CGRect (0, 0, 150, 24));
			ComboBoxEditor.TranslatesAutoresizingMaskIntoConstraints = false;
			ComboBoxEditor.BackgroundColor = NSColor.Clear;
			ComboBoxEditor.StringValue = string.Empty;
			ComboBoxEditor.Cell.ControlSize = NSControlSize.Regular;

			AddSubview (ComboBoxEditor);
		}


		protected override void HandleErrorsChanged (object sender, DataErrorsChangedEventArgs e)
		{
			UpdateErrorsDisplayed (ViewModel.GetErrors (ViewModel.Property.Name));
		}

		protected override void HandlePropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			Debug.WriteLine ("Proerty Name : {0}", e.PropertyName);
			/* var type = ViewModel.Property.Type;
			if (e.PropertyName == nameof (EnumPropertyViewModel<type>.Value)) {
				UpdateModelValue ();
			}*/
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
			var property = ViewModel.Property;

			if (property.Type.IsEnum && ComboBoxEditor.Count == 0) {
				var enumValues = Enum.GetValues (ViewModel.Property.Type);

				foreach (var item in enumValues) {
					ComboBoxEditor.Add (new NSString (item.ToString ()));
				}
			}

			base.UpdateModelValue ();
		}
	}
}