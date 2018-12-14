﻿using System;
using System.Collections;
using System.Collections.Generic;
using AppKit;
using CoreGraphics;
using Foundation;
using Xamarin.PropertyEditing.Mac.Resources;
using Xamarin.PropertyEditing.ViewModels;
using System.IO;

namespace Xamarin.PropertyEditing.Mac
{
	internal class FilePathEditorControl : PropertyEditorControl<PropertyViewModel<FilePath>>
	{
		readonly NSOpenPanel panel;
		readonly TextFieldSmallButtonContainer currentTextField;
		readonly SmallButton browsePathButton;
		readonly SmallButton revealPathButton;

		public override NSView FirstKeyView => this.currentTextField;
		public override NSView LastKeyView => this.revealPathButton.Enabled ? this.revealPathButton : this.browsePathButton;

		bool IsDirectory () => ViewModel.Value?.IsDirectory ?? false;
		bool FileExists () => IsDirectory () ? Directory.Exists (this.currentTextField.StringValue) : File.Exists (this.currentTextField.StringValue);

		public FilePathEditorControl (IHostResourceProvider hostResource)
			: base (hostResource)
		{
			this.currentTextField = new TextFieldSmallButtonContainer ();
			this.currentTextField.Changed += CurrentTextField_Changed;
			AddSubview (this.currentTextField);

			#region Reveal handler

			this.revealPathButton = new SmallButton {
				Font = NSFont.FromFontName (DefaultFontName, DefaultFontSize),
				StringValue = string.Empty,
				Image = hostResource.GetNamedImage ("path-reveal"),
			};
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
			this.revealPathButton.Activated += revealPathButton_Activated;

			#endregion

			#region Browse Path

			this.browsePathButton = new SmallButton {
				Font = NSFont.FromFontName (DefaultFontName, DefaultFontSize),
				StringValue = string.Empty,
				Image = hostResource.GetNamedImage ("path-browse"),
			};
			this.browsePathButton.Activated += BrowsePathButton_Activated;
			this.currentTextField.AddButton (this.browsePathButton);

			#endregion

			AddConstraints (new[] {
				NSLayoutConstraint.Create (this.currentTextField, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1, 20),
				NSLayoutConstraint.Create (this.currentTextField, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, this, NSLayoutAttribute.CenterY, 1, 0),
				NSLayoutConstraint.Create (this.currentTextField, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this, NSLayoutAttribute.Width, 1, -34),
			});
		}

		void CurrentTextField_Changed (object sender, EventArgs e)
		{
			StoreCurrentValue ();
		}

		void StoreCurrentValue ()
		{
			if (ViewModel.Value == null) {
				return;
			}
			ViewModel.Value = new FilePath { Source = this.currentTextField.StringValue, IsDirectory = IsDirectory () };
		}

		void BrowsePathButton_Activated (object sender, EventArgs e)
		{
			this.panel.BeginSheet (this.Window, HandleAction);
		}

		void revealPathButton_Activated (object sender, EventArgs e)
		{
			if (FileExists ()) {
				NSWorkspace.SharedWorkspace.OpenFile (this.currentTextField.StringValue);
			}
		}

		void HandleAction (nint result)
		{
			if (result == 1) {
				var url = this.panel.Url;
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

			if (IsDirectory ()) {
				this.currentTextField.ToolTip = this.currentTextField.PlaceholderString = string.Format (LocalizationResources.ChooseFileOrDirectory, LocalizationResources.Directory);
				this.revealPathButton.ToolTip = string.Format (LocalizationResources.RevealFileOrDirectory, LocalizationResources.Directory);
				this.browsePathButton.ToolTip = string.Format (LocalizationResources.BrowseFileOrDirectory, LocalizationResources.Directory);

				this.panel.CanChooseFiles = false;
				this.panel.CanChooseDirectories = true;
			} else {
				this.currentTextField.ToolTip = this.currentTextField.PlaceholderString = string.Format (LocalizationResources.ChooseFileOrDirectory, LocalizationResources.File);
				this.revealPathButton.ToolTip = string.Format (LocalizationResources.RevealFileOrDirectory, LocalizationResources.File);
				this.browsePathButton.ToolTip = string.Format (LocalizationResources.BrowseFileOrDirectory, LocalizationResources.File);

				this.panel.CanChooseFiles = true;
				this.panel.CanChooseDirectories = false;
			}
			//button states
			this.revealPathButton.Enabled = FileExists ();
			Window?.RecalculateKeyViewLoop ();
		}

		protected override void SetEnabled ()
		{
			this.currentTextField.Enabled = ViewModel.Property.CanWrite;
		}

		protected override void UpdateAccessibilityValues ()
		{
			this.currentTextField.AccessibilityTitle = string.Format (LocalizationResources.AccessibilityString, ViewModel.Property.Name);
		}

		protected override void Dispose (bool disposing)
		{
			this.currentTextField.Changed -= CurrentTextField_Changed;
			this.browsePathButton.Activated -= BrowsePathButton_Activated;
			this.revealPathButton.Activated -= revealPathButton_Activated;
			base.Dispose (disposing);
		}
	}
}
