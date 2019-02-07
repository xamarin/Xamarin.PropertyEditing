using System;
using System.Collections;
using AppKit;
using Xamarin.PropertyEditing.Mac.Resources;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class DateTimeEditorControl : PropertyEditorControl<PropertyViewModel<DateTime>>
	{
		private const int HeightMargin = 1;

		private readonly NSDatePicker editor;

		public DateTimeEditorControl (IHostResourceProvider hostResources)
			: base (hostResources)
		{
			this.editor = new NSDatePicker {
				TranslatesAutoresizingMaskIntoConstraints = false,
				Font = NSFont.FromFontName (DefaultFontName, DefaultFontSize),
				DatePickerElements = NSDatePickerElementFlags.HourMinuteSecond | NSDatePickerElementFlags.YearMonthDateDay
			};

			// update the value on keypress
			this.editor.Activated += Editor_Activated;

			AddSubview (this.editor);

			AddConstraints (new[] {
				NSLayoutConstraint.Create (this.editor, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Top, 1f, 1f),
				NSLayoutConstraint.Create (this.editor, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Left, 1f, 0),
				NSLayoutConstraint.Create (this.editor, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, DefaultControlHeight),
			});
		}

		private void Editor_Activated (object sender, EventArgs e) =>
			ViewModel.Value = this.editor.DateValue.ToDateTime ();

		public override NSView FirstKeyView => this.editor;
		public override NSView LastKeyView => this.editor;

		public override nint GetHeight (EditorViewModel vm)
		{
			return DefaultControlHeight + HeightMargin;
		}

		protected override void UpdateValue ()
		{
			this.editor.DateValue = ViewModel.Value.ToNSDate ();
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
			this.editor.Enabled = ViewModel.Property.CanWrite;
		}

		protected override void UpdateAccessibilityValues ()
		{
			this.editor.AccessibilityTitle = string.Format (LocalizationResources.AccessibilityDateTime, ViewModel.Property.Name);
		}

		protected override void Dispose (bool disposing)
		{
			this.editor.Activated -= Editor_Activated;
			base.Dispose (disposing);
		}
	}
}
