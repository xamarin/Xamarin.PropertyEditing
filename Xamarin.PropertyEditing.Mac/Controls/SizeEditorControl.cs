using System;
using System.Drawing;
using CoreGraphics;

namespace Xamarin.PropertyEditing.Mac
{
	internal class SizeEditorControl : BasePointEditorControl<Size>
	{
		public SizeEditorControl ()
		{
			XLabel.Frame = new CGRect (0, 0, 40, 24);
			XLabel.StringValue = "Width:"; // TODO Localise

			XEditor.Frame = new CGRect (45, 0, 50, 20);

			YLabel.Frame = new CGRect (105, 0, 45, 24);
			YLabel.StringValue = "Height:"; // TODO Localise

			YEditor.Frame = new CGRect (155, 0, 50, 20);
		}

		protected override void UpdateValue ()
		{
			XEditor.StringValue = ViewModel.Value.Width.ToString ();
			YEditor.StringValue = ViewModel.Value.Height.ToString ();
		}

		protected override void OnInputUpdated (object sender, EventArgs e)
		{
			ViewModel.Value = new Size (XEditor.IntValue, YEditor.IntValue);
		}

		protected override void UpdateAccessibilityValues ()
		{
			XEditor.AccessibilityTitle = ViewModel.Property.Name + " Width Editor"; // TODO Localization
			YEditor.AccessibilityTitle = ViewModel.Property.Name + " Height Editor"; // TODO Localization
		}
	}
}
