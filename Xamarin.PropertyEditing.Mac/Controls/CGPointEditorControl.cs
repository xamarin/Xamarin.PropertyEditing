using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using AppKit;
using CoreGraphics;
using Foundation;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class CGPointEditorControl : BasePointEditorControl<CGPoint>
	{
		public CGPointEditorControl ()
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
			XEditor.Value = ViewModel.Value.X;
			YEditor.Value = ViewModel.Value.Y;
		}

		protected override void OnInputUpdated (object sender, EventArgs e)
		{
			ViewModel.Value = new CGPoint (XEditor.Value, YEditor.Value);
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
