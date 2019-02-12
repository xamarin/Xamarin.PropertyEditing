using System;
using System.Collections;
using AppKit;
using Foundation;
using Xamarin.PropertyEditing.Mac.Resources;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal abstract class BasePathEditorControl<T> : PropertyEditorControl<PropertyViewModel<T>>
	{
		protected readonly TextFieldSmallButtonContainer currentTextField;

		protected readonly NSOpenPanel panel;
		protected readonly SmallButton browsePathButton;
		protected readonly SmallButton revealPathButton;

		public override NSView FirstKeyView => this.currentTextField;
		public override NSView LastKeyView => this.revealPathButton.Enabled ? this.revealPathButton : this.browsePathButton;

		protected BasePathEditorControl (IHostResourceProvider hostResource)
			: base (hostResource)
		{
			this.currentTextField = new TextFieldSmallButtonContainer ();
			this.currentTextField.Changed += CurrentTextField_Changed;
			AddSubview (this.currentTextField);

			#region Reveal handler

			this.revealPathButton = new SmallButton {
				Font = NSFont.FromFontName (DefaultFontName, DefaultFontSize),
				StringValue = string.Empty
			};

			this.currentTextField.AddButton (this.revealPathButton);

			this.panel = new NSOpenPanel {
				ShowsResizeIndicator = true,
				ShowsHiddenFiles = false,
				CanCreateDirectories = true,
				AllowsMultipleSelection = false
			};

			// update the value on keypress
			this.revealPathButton.Activated += OnRevealPathButtonActivated;

			#endregion

			#region Browse Path

			this.browsePathButton = new SmallButton {
				Font = NSFont.FromFontName (DefaultFontName, DefaultFontSize),
				StringValue = string.Empty
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
			this.revealPathButton.Image = HostResources.GetNamedImage ("path-reveal");
			this.browsePathButton.Image = HostResources.GetNamedImage ("path-browse");
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

		protected override void Dispose (bool disposing)
		{
			this.currentTextField.Changed -= CurrentTextField_Changed;
			this.browsePathButton.Activated -= BrowsePathButton_Activated;
			this.revealPathButton.Activated -= OnRevealPathButtonActivated;
			base.Dispose (disposing);
		}
	}
}
