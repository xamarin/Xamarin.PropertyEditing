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
	internal class CGSizeEditorControl : BasePointEditorControl<CGSize>
	{
		public CGSizeEditorControl ()
		{
			XLabel.Frame = new CGRect (0, 0, 40, 24);
			XLabel.StringValue = "Width:";

			XEditor.Frame = new CGRect (45, 0, 50, 20);

			YLabel.Frame = new CGRect (105, 0, 45, 24);
			YLabel.StringValue = "Height:";

			YEditor.Frame = new CGRect (155, 0, 50, 20);
		}

		protected override void UpdateValue ()
		{
			XEditor.Value = ViewModel.Value.Width;
			YEditor.Value = ViewModel.Value.Height;
		}

		protected override void OnInputUpdated (object sender, EventArgs e)
		{
			ViewModel.Value = new CGSize (XEditor.Value, YEditor.Value);
		}

		protected override void UpdateAccessibilityValues ()
		{
			XEditor.AccessibilityEnabled = XEditor.Enabled;
			XEditor.AccessibilityTitle = Strings.AccessibilityWidthEditor (ViewModel.Property.Name);

			YEditor.AccessibilityEnabled = YEditor.Enabled;
			YEditor.AccessibilityTitle = Strings.AccessibilityHeightEditor (ViewModel.Property.Name);
		}
	}
}
