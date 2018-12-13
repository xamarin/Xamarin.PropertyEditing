using System;
using System.Collections;
using AppKit;
using Xamarin.PropertyEditing.Mac.Resources;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class TimeSpanEditorControl : PropertyEditorControl<PropertyViewModel<TimeSpan>>
	{
		readonly TimeSpanTextField editor;

		public TimeSpanEditorControl ()
		{
			editor = new TimeSpanTextField {
				Font = NSFont.FromFontName (DefaultFontName, DefaultFontSize),
			};

			// update the value on keypress
			editor.Changed += (sender, e) => ViewModel.Value = TimeSpan.Parse (editor.StringValue);

			AddSubview (editor);

			this.AddConstraints (new[] {
				NSLayoutConstraint.Create (editor, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Top, 1f, 1f),
				NSLayoutConstraint.Create (editor, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Width, 1f, -34f),
				NSLayoutConstraint.Create (editor, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, DefaultControlHeight - 3),
			});

			UpdateTheme ();
		}

		public override NSView FirstKeyView => editor;
		public override NSView LastKeyView => editor;

		protected override void UpdateValue ()
		{
			editor.StringValue = ViewModel.Value.ToString ();
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
			editor.Editable = ViewModel.Property.CanWrite;
		}

		protected override void UpdateAccessibilityValues ()
		{
			editor.AccessibilityTitle = string.Format (LocalizationResources.AccessibilityTimeSpan, ViewModel.Property.Name);
		}
	}
}
