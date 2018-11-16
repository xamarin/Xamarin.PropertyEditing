using System;
using System.Collections;
using System.Diagnostics;
using AppKit;
using CoreGraphics;
using Xamarin.PropertyEditing.Mac.Resources;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class StringEditorControl : PropertyEditorControl<PropertyViewModel<string>>
	{
		public StringEditorControl ()
		{
			StringEditor = new NSTextField {
				BackgroundColor = NSColor.Clear,
				ControlSize = NSControlSize.Small,
				Font = NSFont.FromFontName (DefaultFontName, DefaultFontSize),
				StringValue = string.Empty,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			// update the value on keypress
			StringEditor.Changed += (sender, e) => {
				ViewModel.Value = StringEditor.StringValue;
			};
			AddSubview (StringEditor);

			this.DoConstraints (new[] {
				StringEditor.ConstraintTo (this, (s, c) => s.Width == c.Width - 34),
				StringEditor.ConstraintTo (this, (s, c) => s.Height == DefaultControlHeight - 3),
				StringEditor.ConstraintTo (this, (s, c) => s.Top == s.Top + 1),
				StringEditor.ConstraintTo (this, (s, c) => s.Left == s.Left - 1),
			});

			UpdateTheme ();
		}

		internal NSTextField StringEditor { get; set; }

		public override NSView FirstKeyView => StringEditor;
		public override NSView LastKeyView => StringEditor;

		protected override void UpdateValue ()
		{
			StringEditor.StringValue = ViewModel.Value ?? string.Empty;
		}

		protected override void HandleErrorsChanged (object sender, System.ComponentModel.DataErrorsChangedEventArgs e)
		{
			UpdateErrorsDisplayed (ViewModel.GetErrors (ViewModel.Property.Name));
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

		protected override void SetEnabled ()
		{
			StringEditor.Editable = ViewModel.Property.CanWrite;
		}

		protected override void UpdateAccessibilityValues ()
		{
			StringEditor.AccessibilityTitle = string.Format (LocalizationResources.AccessibilityString, ViewModel.Property.Name);
		}
	}
}
