using System.Collections;
using AppKit;
using Xamarin.PropertyEditing.Mac.Resources;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class CharEditorControl : PropertyEditorControl<PropertyViewModel<char>>
	{
		public CharEditorControl ()
		{
			StringEditor = new CharTextField {
				BackgroundColor = NSColor.Clear,
				ControlSize = NSControlSize.Small,
				Font = NSFont.FromFontName (DefaultFontName, DefaultFontSize),
				StringValue = string.Empty,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			// update the value on keypress
			StringEditor.Changed += (sender, e) => {
				if (StringEditor.StringValue.Length > 0) {
					ViewModel.Value = StringEditor.StringValue[0];
				} else {
					ViewModel.Value = default (char);
				}
			};
			AddSubview (StringEditor);

			this.AddConstraints (new[] {
				NSLayoutConstraint.Create (StringEditor, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Top, 1f, 1f),
				NSLayoutConstraint.Create (StringEditor, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Width, 1f, -34f),
				NSLayoutConstraint.Create (StringEditor, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, DefaultControlHeight - 3),
			});

			UpdateTheme ();
		}

		internal CharTextField StringEditor { get; set; }

		public override NSView FirstKeyView => StringEditor;
		public override NSView LastKeyView => StringEditor;

		protected override void UpdateValue ()
		{
			StringEditor.StringValue = ViewModel.Value.ToString ();
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
