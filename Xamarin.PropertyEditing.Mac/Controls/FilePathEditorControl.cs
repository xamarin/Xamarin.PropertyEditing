using System;
using System.Collections;
using System.Collections.Generic;
using AppKit;
using CoreGraphics;
using Foundation;
using Xamarin.PropertyEditing.Mac.Resources;
using Xamarin.PropertyEditing.ViewModels;
using System.IO;
using Xamarin.PropertyEditing.Common;

namespace Xamarin.PropertyEditing.Mac
{
	internal class FilePathEditorControl : PropertyEditorControl<PropertyViewModel<FilePath>>
	{
		private readonly NSOpenPanel panel;
		private readonly TextFieldSmallButtonContainer currentTextField;
		private readonly SmallButton browsePathButton;
		private readonly SmallButton revealPathButton;

		public override NSView FirstKeyView => this.currentTextField;
		public override NSView LastKeyView => this.revealPathButton.Enabled ? this.revealPathButton : this.browsePathButton;

		private bool FileExists () => File.Exists (this.currentTextField.StringValue);

		public FilePathEditorControl (IHostResourceProvider hostResource)
			: base (hostResource)
		{
			this.currentTextField = new TextFieldSmallButtonContainer ();
			this.currentTextField.ToolTip = this.currentTextField.PlaceholderString = string.Format (LocalizationResources.ChooseFileOrDirectory, LocalizationResources.File);
			this.currentTextField.Changed += CurrentTextField_Changed;
			AddSubview (this.currentTextField);

			#region Reveal handler

			this.revealPathButton = new SmallButton {
				Font = NSFont.FromFontName (DefaultFontName, DefaultFontSize),
				StringValue = string.Empty,
				Image = hostResource.GetNamedImage ("path-reveal"),
			};

			this.revealPathButton.ToolTip = string.Format (LocalizationResources.RevealFileOrDirectory, LocalizationResources.File);

			this.currentTextField.AddButton (this.revealPathButton);

			this.panel = new NSOpenPanel {
				Title = string.Format (LocalizationResources.ChooseFileOrDirectory, LocalizationResources.File),
				CanChooseFiles = true,
				ShowsResizeIndicator = true,
				ShowsHiddenFiles = false,
				CanChooseDirectories = false,
				CanCreateDirectories = true,
				AllowsMultipleSelection = false
			};

			// update the value on keypress
			this.revealPathButton.Activated += RevealPathButton_Activated;

			#endregion

			#region Browse Path

			this.browsePathButton = new SmallButton {
				Font = NSFont.FromFontName (DefaultFontName, DefaultFontSize),
				StringValue = string.Empty,
				Image = hostResource.GetNamedImage ("path-browse"),
			};
			this.browsePathButton.Activated += BrowsePathButton_Activated;
			this.currentTextField.AddButton (this.browsePathButton);

			this.browsePathButton.ToolTip = string.Format (LocalizationResources.BrowseFileOrDirectory, LocalizationResources.File);

			#endregion

			AddConstraints (new[] {
				NSLayoutConstraint.Create (this.currentTextField, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1, 20),
				NSLayoutConstraint.Create (this.currentTextField, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, this, NSLayoutAttribute.CenterY, 1, 0),
				NSLayoutConstraint.Create (this.currentTextField, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this, NSLayoutAttribute.Width, 1, -34),
			});
		}

		private void CurrentTextField_Changed (object sender, EventArgs e)
		{
			StoreCurrentValue ();
		}

		private void StoreCurrentValue ()
		{
			if (ViewModel.Value == null) {
				return;
			}
			ViewModel.Value = new FilePath { Source = this.currentTextField.StringValue };
		}

		private void BrowsePathButton_Activated (object sender, EventArgs e)
		{
			this.panel.BeginSheet (this.Window, HandleAction);
		}

		private void RevealPathButton_Activated (object sender, EventArgs e)
		{
			if (FileExists ()) {
				NSWorkspace.SharedWorkspace.OpenFile (this.currentTextField.StringValue);
			}
		}

		private void HandleAction (nint result)
		{
			if (result == 1) {
				NSUrl url = this.panel.Url;
				if (url.IsFileUrl) {
					this.currentTextField.StringValue = url.Path;
					StoreCurrentValue ();
				}
			}
		}

		protected override void UpdateValue ()
		{
			if (ViewModel.Value == null) {
				this.revealPathButton.Alignment = NSTextAlignment.Center;
				this.currentTextField.StringValue = "";
			} else {
				this.currentTextField.StringValue = ViewModel.Value.Source ?? string.Empty;
				this.revealPathButton.Alignment = NSTextAlignment.Left;
			}

			//button states
			this.revealPathButton.Enabled = FileExists ();
			Window?.RecalculateKeyViewLoop ();
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
			this.currentTextField.Enabled = ViewModel.Property.CanWrite;
		}

		protected override void UpdateAccessibilityValues ()
		{
			this.currentTextField.AccessibilityTitle = string.Format (LocalizationResources.AccessibilityPathEditor, LocalizationResources.File);
		}

		protected override void Dispose (bool disposing)
		{
			this.currentTextField.Changed -= CurrentTextField_Changed;
			this.browsePathButton.Activated -= BrowsePathButton_Activated;
			this.revealPathButton.Activated -= RevealPathButton_Activated;
			base.Dispose (disposing);
		}
	}
}
