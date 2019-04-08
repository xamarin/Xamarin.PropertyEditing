using System;
using System.Collections;
using AppKit;
using Foundation;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal abstract class BasePathEditorControl<T> : PropertyEditorControl<PropertyViewModel<T>>
	{
		private const string PathRevealName = "pe-path-reveal";
		private const string PathBrowseName = "pe-path-browse";

		protected readonly TextFieldSmallButtonContainer currentTextField;

		protected readonly NSOpenPanel panel;
		protected readonly SmallButton browsePathButton;
		protected readonly SmallButton revealPathButton;

		public override NSView FirstKeyView => this.currentTextField;
		public override NSView LastKeyView => this.revealPathButton.Enabled ? this.revealPathButton : this.browsePathButton;

		private readonly NSObject[] objects;
		public override NSObject[] AccessibilityChildren { get => this.objects; }

		protected BasePathEditorControl (IHostResourceProvider hostResource)
			: base (hostResource)
		{
			this.currentTextField = new TextFieldSmallButtonContainer ();
			this.currentTextField.Changed += CurrentTextField_Changed;
			AddSubview (this.currentTextField);

			#region Reveal handler

			this.revealPathButton = new SmallButton {
				Font = NSFont.FromFontName (DefaultFontName, DefaultFontSize),
				Image = HostResources.GetNamedImage (PathRevealName),
				StringValue = string.Empty
			};

			this.currentTextField.AddButton (this.revealPathButton);

			this.panel = new NSOpenPanel {
				AllowsMultipleSelection = false,
				CanCreateDirectories = true,
				ShowsHiddenFiles = false,
				ShowsResizeIndicator = true,
				TreatsFilePackagesAsDirectories = true,
			};

			// update the value on keypress
			this.revealPathButton.Activated += OnRevealPathButtonActivated;

			#endregion

			#region Browse Path

			this.browsePathButton = new SmallButton {
				Font = NSFont.FromFontName (DefaultFontName, DefaultFontSize),
				Image = HostResources.GetNamedImage (PathBrowseName),
				StringValue = string.Empty
			};
			this.browsePathButton.Activated += BrowsePathButton_Activated;
			this.currentTextField.AddButton (this.browsePathButton);

			this.objects = new NSObject[3];
			this.objects[0] = this.currentTextField;
			this.objects[1] = this.browsePathButton;
			this.objects[2] = this.revealPathButton;
			this.AccessibilityElement = true;
			this.AccessibilityRole = NSAccessibilityRoles.GroupRole;

			this.currentTextField.AccessibilityRole = NSAccessibilityRoles.TextFieldRole;
			this.revealPathButton.AccessibilityRole = NSAccessibilityRoles.ButtonRole;
			this.browsePathButton.AccessibilityRole = NSAccessibilityRoles.ButtonRole;

			#endregion

			AddConstraints (new[] {
				NSLayoutConstraint.Create (this.currentTextField, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this, NSLayoutAttribute.Left, 1, 0f),
				NSLayoutConstraint.Create (this.currentTextField, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, this, NSLayoutAttribute.CenterY, 1, 0f),
				NSLayoutConstraint.Create (this.currentTextField, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this, NSLayoutAttribute.Width, 1, 0f),
				NSLayoutConstraint.Create (this.currentTextField, NSLayoutAttribute.Height, NSLayoutRelation.Equal, this, NSLayoutAttribute.Height, 1, -6f),
			});
		}

		private void CurrentTextField_Changed (object sender, EventArgs e)
		{
			StoreCurrentValue ();
		}

		private void BrowsePathButton_Activated (object sender, EventArgs e)
		{
			this.panel.BeginSheet (this.Window, HandleAction);
		}

		public override void ViewDidChangeEffectiveAppearance ()
		{
			this.revealPathButton.Image = HostResources.GetNamedImage (PathRevealName);
			this.browsePathButton.Image = HostResources.GetNamedImage (PathBrowseName);
			base.ViewDidChangeEffectiveAppearance ();
		}

		protected abstract void OnRevealPathButtonActivated (object sender, EventArgs e);
		protected abstract void StoreCurrentValue ();

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

		protected override void SetEnabled ()
		{
			this.currentTextField.Enabled =
			this.browsePathButton.Enabled =
			this.revealPathButton.Enabled = ViewModel.Property.CanWrite;
		}

		protected override void Dispose (bool disposing)
		{
			this.currentTextField.Changed -= CurrentTextField_Changed;
			this.browsePathButton.Activated -= BrowsePathButton_Activated;
			this.revealPathButton.Activated -= OnRevealPathButtonActivated;
			base.Dispose (disposing);
		}
	}
}
