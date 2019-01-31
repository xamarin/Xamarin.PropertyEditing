using System.Collections;
using AppKit;
using Xamarin.PropertyEditing.Mac.Resources;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class FilePathEditorControl : PropertyEditorControl<PropertyViewModel<FilePath>>
	{
		const string SelectFileTitle = "Choose a File";

		NSOpenPanel panel;

		public FilePathEditorControl (IHostResourceProvider hostResources)
			: base (hostResources)
		{
			ButtonEditor = new NSButton {
				BezelStyle = NSBezelStyle.RegularSquare,
				ControlSize = NSControlSize.Small,
				Font = NSFont.FromFontName (DefaultFontName, DefaultFontSize),
				StringValue = string.Empty,
				Bordered = false,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			panel = new NSOpenPanel {
				Title = SelectFileTitle,
				CanChooseFiles = true,
				ShowsResizeIndicator = true,
				ShowsHiddenFiles = false,
				CanChooseDirectories = false,
				CanCreateDirectories = true,
				AllowsMultipleSelection = false
			};

			// update the value on keypress
			ButtonEditor.Activated += ButtonEditor_Activated;
			AddSubview (ButtonEditor);

			this.AddConstraints (new[] {
				NSLayoutConstraint.Create (ButtonEditor, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Top, 1f, 1f),
				NSLayoutConstraint.Create (ButtonEditor, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Width, 1f, -34f),
				NSLayoutConstraint.Create (ButtonEditor, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, DefaultControlHeight - 3),
			});

		}

		void ButtonEditor_Activated (object sender, System.EventArgs e)
		{
			panel.BeginSheet (this.Window, HandleAction);
		}

		void HandleAction (System.nint result)
		{
			if (result == 1) {
				var url = panel.Url;
				if (url.IsFileUrl) {
					ViewModel.Value = new FilePath (url.Path);
					ButtonEditor.Alignment = NSTextAlignment.Left;
				}
			}
		}

		internal NSButton ButtonEditor { get; set; }

		public override NSView FirstKeyView => ButtonEditor;
		public override NSView LastKeyView => ButtonEditor;

		protected override void UpdateValue ()
		{
			if (ViewModel.Value == null) {
				ButtonEditor.Alignment = NSTextAlignment.Center;
				ButtonEditor.Title = string.Format ("{0}...", SelectFileTitle);
			} else {
				ButtonEditor.Title = ViewModel.Value.Source;
				ButtonEditor.Alignment = NSTextAlignment.Left;
			}
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
			ButtonEditor.Enabled = ViewModel.Property.CanWrite;
		}

		protected override void UpdateAccessibilityValues ()
		{
			ButtonEditor.AccessibilityTitle = string.Format (LocalizationResources.AccessibilityString, ViewModel.Property.Name);
		}

		protected override void Dispose (bool disposing)
		{
			ButtonEditor.Activated -= ButtonEditor_Activated;
			base.Dispose (disposing);
		}
	}
}
