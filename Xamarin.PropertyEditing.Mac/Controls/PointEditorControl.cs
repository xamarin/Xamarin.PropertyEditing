using System;
using System.Drawing;
using CoreGraphics;

namespace Xamarin.PropertyEditing.Mac
{
	internal class PointEditorControl : BasePointEditorControl<Point>
	{
		public PointEditorControl ()
		{
			XLabel.Frame = new CGRect (0, 0, 25, 24);
			XLabel.StringValue = "X:"; // TODO Localise

			XEditor.Frame = new CGRect (25, 0, 50, 20);

			YLabel.Frame = new CGRect (85, 0, 25, 24);
			YLabel.StringValue = "Y:"; // TODO Localise

			YEditor.Frame = new CGRect (110, 0, 50, 20);
		}

		protected override void UpdateValue ()
		{
			XEditor.StringValue = ViewModel.Value.X.ToString ();
			YEditor.StringValue = ViewModel.Value.Y.ToString ();
		}

		protected override void OnInputUpdated (object sender, EventArgs e)
		{
			ViewModel.Value = new Point (XEditor.IntValue, YEditor.IntValue);
		}

		protected override void UpdateAccessibilityValues ()
		{
			XEditor.AccessibilityEnabled = XEditor.Enabled;
			XEditor.AccessibilityTitle = ViewModel.Property.Name + " X Editor"; // TODO Localization

			YEditor.AccessibilityEnabled = YEditor.Enabled;
			YEditor.AccessibilityTitle = ViewModel.Property.Name + " Y Editor"; // TODO Localization
		}
	}
}
