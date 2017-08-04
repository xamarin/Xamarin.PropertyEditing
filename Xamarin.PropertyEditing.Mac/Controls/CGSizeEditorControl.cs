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
	internal class CGSizeEditorControl : BasePointEditorControl<CGSize>
	{
		public CGSizeEditorControl ()
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
			ViewModel.Value = new CGSize (XEditor.FloatValue, YEditor.FloatValue);
		}

		protected override void UpdateAccessibilityValues ()
		{
			XEditor.AccessibilityEnabled = XEditor.Enabled;
			XEditor.AccessibilityTitle = ViewModel.Property.Name + " Width Editor"; // TODO Localization

			YEditor.AccessibilityEnabled = YEditor.Enabled;
			YEditor.AccessibilityTitle = ViewModel.Property.Name + " Height Editor"; // TODO Localization
		}
	}
}
