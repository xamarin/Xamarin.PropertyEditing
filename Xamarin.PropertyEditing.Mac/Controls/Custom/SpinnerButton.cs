using System;
using AppKit;
using Xamarin.PropertyEditing.Themes;

namespace Xamarin.PropertyEditing.Mac
{
	public class UpSpinnerButton : UnfocusableButton
	{
		public UpSpinnerButton ()
		{
			OnMouseEntered += (sender, e) => {
				switch (PropertyEditorPanel.ThemeManager.Theme) {
					case PropertyEditorTheme.Dark:
						Image = NSImage.ImageNamed ("stepper-up-focus-blue~dark");
						break;

					case PropertyEditorTheme.Light:
						Image = NSImage.ImageNamed ("stepper-up-focus-blue");
						break;

					case PropertyEditorTheme.None:
						Image = NSImage.ImageNamed ("stepper-up-focus-graphite");
						break;
				}
			};

			OnMouseExited += (sender, e) => {
				switch (PropertyEditorPanel.ThemeManager.Theme) {
					case PropertyEditorTheme.Dark:
						Image = NSImage.ImageNamed ("stepper-up~dark");
						break;

					case PropertyEditorTheme.Light:
						Image = NSImage.ImageNamed ("stepper-up");
						break;

					case PropertyEditorTheme.None:
						Image = NSImage.ImageNamed ("stepper-up");
						break;
				}
			};
		}

		protected override void UpdateTheme ()
		{
			base.UpdateTheme ();

			switch (PropertyEditorPanel.ThemeManager.Theme) {
				case PropertyEditorTheme.Dark:
					Image = NSImage.ImageNamed ("stepper-up~dark");
					break;

				case PropertyEditorTheme.Light:
					Image = NSImage.ImageNamed ("stepper-up");
					break;

				case PropertyEditorTheme.None:
					Image = NSImage.ImageNamed ("stepper-up");
					break;
			}
		}
	}

	public class DownSpinnerButton : UnfocusableButton
	{
		public DownSpinnerButton ()
		{
			OnMouseEntered += (sender, e) => {
				switch (PropertyEditorPanel.ThemeManager.Theme) {
					case PropertyEditorTheme.Dark:
						Image = NSImage.ImageNamed ("stepper-down-focus-blue~dark");
						break;

					case PropertyEditorTheme.Light:
						Image = NSImage.ImageNamed ("stepper-down-focus-blue");
						break;

					case PropertyEditorTheme.None:
						Image = NSImage.ImageNamed ("stepper-down-focus-graphite");
						break;
				}
			};

			OnMouseExited += (sender, e) => {
				switch (PropertyEditorPanel.ThemeManager.Theme) {
					case PropertyEditorTheme.Dark:
						Image = NSImage.ImageNamed ("stepper-down~dark");
						break;

					case PropertyEditorTheme.Light:
						Image = NSImage.ImageNamed ("stepper-down");
						break;

					case PropertyEditorTheme.None:
						Image = NSImage.ImageNamed ("stepper-down");
						break;
				}
			};
		}

		protected override void UpdateTheme ()
		{
			base.UpdateTheme ();

			switch (PropertyEditorPanel.ThemeManager.Theme) {
				case PropertyEditorTheme.Dark:
					Image = NSImage.ImageNamed ("stepper-down~dark");
					break;

				case PropertyEditorTheme.Light:
					Image = NSImage.ImageNamed ("stepper-down");
					break;

				case PropertyEditorTheme.None:
					Image = NSImage.ImageNamed ("stepper-down");
					break;
			}
		}
	}
}
