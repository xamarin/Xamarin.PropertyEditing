using System;
using System.Collections.Generic;
using AppKit;
using CoreGraphics;
using System.IO;
using Xamarin.PropertyEditing.Common;
using Foundation;

namespace Xamarin.PropertyEditing.Mac
{
	internal class FilePathEditorControl : BasePathEditorControl<FilePath>
	{
		public FilePathEditorControl (IHostResourceProvider hostResource)
		: base (hostResource)
		{
			this.currentTextField.ToolTip = this.currentTextField.PlaceholderString = string.Format (Properties.Resources.ChooseFileOrDirectory, Properties.Resources.File);
			this.panel.CanChooseFiles = true;
			this.panel.CanChooseDirectories = false;
			this.revealPathButton.ToolTip = string.Format (Properties.Resources.RevealFileOrDirectory, Properties.Resources.File);
			this.browsePathButton.ToolTip = string.Format (Properties.Resources.BrowseFileOrDirectory, Properties.Resources.File);
			this.panel.Title = string.Format (Properties.Resources.ChooseFileOrDirectory, Properties.Resources.File);
		}

		protected override void OnRevealPathButtonActivated (object sender, EventArgs e)
		{
			Window.MakeFirstResponder (this.currentTextField);
			if (File.Exists (this.currentTextField.StringValue)) {
				NSWorkspace.SharedWorkspace.SelectFile (this.currentTextField.StringValue, string.Empty);
			}
		}

		protected override void StoreCurrentValue ()
		{
			ViewModel.Value = new FilePath (this.currentTextField.StringValue);
		}

		protected override void UpdateAccessibilityValues ()
		{
			this.currentTextField.AccessibilityEnabled = this.currentTextField.Enabled;
			this.currentTextField.AccessibilityTitle = string.Format (Properties.Resources.AccessibilityPathEditor, ViewModel.Property.Name, Properties.Resources.File);

			this.revealPathButton.AccessibilityEnabled = this.browsePathButton.AccessibilityEnabled = this.currentTextField.AccessibilityEnabled;
			this.revealPathButton.AccessibilityTitle = string.Format (Properties.Resources.AccessibilityRevealButton, ViewModel.Property.Name, Properties.Resources.File);
			this.browsePathButton.AccessibilityTitle = string.Format (Properties.Resources.AccessibilityBrowseButton, ViewModel.Property.Name, Properties.Resources.File);
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

			SetEnabled ();
		}

		protected override void SetEnabled ()
		{
			this.currentTextField.Enabled = ViewModel.Property.CanWrite;
			this.browsePathButton.Enabled = ViewModel.Property.CanWrite;

			//button states
			this.revealPathButton.Enabled = ViewModel.Property.CanWrite && File.Exists (this.currentTextField.StringValue);
			Window?.RecalculateKeyViewLoop ();
		}
	}
}
