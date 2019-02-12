using System;
using System.Collections.Generic;
using AppKit;
using CoreGraphics;
using Xamarin.PropertyEditing.Mac.Resources;
using System.IO;
using Xamarin.PropertyEditing.Common;

namespace Xamarin.PropertyEditing.Mac
{
	internal class FilePathEditorControl : BasePathEditorControl<FilePath>
	{
		public FilePathEditorControl (IHostResourceProvider hostResource)
		: base (hostResource)
		{
			this.currentTextField.ToolTip = this.currentTextField.PlaceholderString = string.Format (LocalizationResources.ChooseFileOrDirectory, LocalizationResources.File);
			this.panel.CanChooseFiles = true;
			this.panel.CanChooseDirectories = false;
			this.revealPathButton.ToolTip = string.Format (LocalizationResources.RevealFileOrDirectory, LocalizationResources.File);
			this.browsePathButton.ToolTip = string.Format (LocalizationResources.BrowseFileOrDirectory, LocalizationResources.File);
			this.panel.Title = string.Format (LocalizationResources.ChooseFileOrDirectory, LocalizationResources.File);
		}

		protected override void OnRevealPathButtonActivated (object sender, EventArgs e)
		{
			if (File.Exists (this.currentTextField.StringValue)) {
				NSWorkspace.SharedWorkspace.OpenFile (this.currentTextField.StringValue);
			}
		}

		protected override void StoreCurrentValue ()
		{
			if (ViewModel.Value == null) {
				return;
			}
			ViewModel.Value = new FilePath () { Source = this.currentTextField.StringValue };
		}

		protected override void UpdateAccessibilityValues ()
		{
			this.currentTextField.AccessibilityEnabled = this.currentTextField.Enabled;
			this.currentTextField.AccessibilityTitle = string.Format (LocalizationResources.AccessibilityPathEditor, LocalizationResources.File);
		}

		protected override void UpdateValue ()
		{
			if (ViewModel.Value == null) {
				this.revealPathButton.Alignment = NSTextAlignment.Center;
				this.currentTextField.StringValue = "";
			} else {
				this.currentTextField.StringValue = ViewModel.Value.Source ?? string.Empty;
				this.revealPathButton.Alignment = NSTextAlignment.Left;

				//button states
				this.revealPathButton.Enabled = File.Exists (this.currentTextField.StringValue);
				Window?.RecalculateKeyViewLoop ();
			}
		}

		protected override void SetEnabled ()
		{
			this.currentTextField.Enabled = ViewModel.Property.CanWrite;
		}
	}
}
