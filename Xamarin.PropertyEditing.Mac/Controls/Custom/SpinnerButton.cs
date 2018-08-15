using System;
using AppKit;
using ObjCRuntime;
using Xamarin.PropertyEditing.Themes;

namespace Xamarin.PropertyEditing.Mac
{
	public class UpSpinnerButton : UnfocusableButton
	{
		public UpSpinnerButton ()
		{
			OnMouseEntered += (sender, e) => {
				Image = PropertyEditorPanel.ThemeManager.GetImageForTheme ("stepper-up-focus-blue");
			};

			OnMouseExited += (sender, e) => {
				Image = PropertyEditorPanel.ThemeManager.GetImageForTheme ("stepper-up");
			};
		}

		protected override void UpdateTheme ()
		{
			base.UpdateTheme ();

			Image = PropertyEditorPanel.ThemeManager.GetImageForTheme ("stepper-up");
		}
	}

	public class DownSpinnerButton : UnfocusableButton
	{
		public DownSpinnerButton ()
		{
			OnMouseEntered += (sender, e) => {
				Image = PropertyEditorPanel.ThemeManager.GetImageForTheme ("stepper-down-focus-blue");
			};

			OnMouseExited += (sender, e) => {
				Image = PropertyEditorPanel.ThemeManager.GetImageForTheme ("stepper-down");
			};
		}

		protected override void UpdateTheme ()
		{
			base.UpdateTheme ();

			Image = PropertyEditorPanel.ThemeManager.GetImageForTheme ("stepper-down");
		}
	}
}
