using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using AppKit;
using CoreGraphics;
using Foundation;
using Xamarin.PropertyEditing.Mac.Resources;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class CGPointEditorControl : BasePointEditorControl<CGPoint>
	{
		public CGPointEditorControl ()
		{
			XLabel.Frame = new CGRect (49, 0, 25, 24);
			XLabel.StringValue = "X:"; // TODO Localise

			XEditor.Frame = new CGRect (48, 0, 50, 20);

			YLabel.Frame = new CGRect (155, 0, 25, 24);
			YLabel.StringValue = "Y:"; // TODO Localise

			YEditor.Frame = new CGRect (175, 0, 50, 20);
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
			XEditor.AccessibilityTitle = string.Format (LocalizationResources.AccessibilityXEditor, ViewModel.Property.Name);

			YEditor.AccessibilityEnabled = YEditor.Enabled;
			YEditor.AccessibilityTitle = string.Format (LocalizationResources.AccessibilityYEditor, ViewModel.Property.Name);
		}
	}
}
