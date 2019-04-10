using System;
using AppKit;
using CoreGraphics;
using Foundation;
using Xamarin.PropertyEditing.Drawing;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class CommonThicknessEditorControl
		: BaseRectangleEditorControl<CommonThickness>
	{
		public CommonThicknessEditorControl (IHostResourceProvider hostResources)
			: base (hostResources)
		{
			XLabel.StringValue = "LEFT";

			XEditor.Frame = new CGRect (0, 46, 90, 20);

			YLabel.StringValue = "TOP";

			YEditor.Frame = new CGRect (132, 46, 90, 20);

			WidthLabel.StringValue = "RIGHT";

			WidthEditor.Frame = new CGRect (0, 13, 90, 20);

			HeightLabel.StringValue = "BOTTOM";

			HeightEditor.Frame = new CGRect (132, 13, 90, 20);
		}

		public override nint GetHeight (EditorViewModel vm)
		{
			return 66;
		}

		protected override void OnInputUpdated (object sender, EventArgs e)
		{
			ViewModel.Value = (CommonThickness)Activator.CreateInstance (typeof (CommonThickness), HeightEditor.Value, XEditor.Value, WidthEditor.Value, YEditor.Value);
		}

		protected override void UpdateAccessibilityValues ()
		{
			XEditor.AccessibilityEnabled = XEditor.Enabled;
			XEditor.AccessibilityTitle = string.Format (Properties.Resources.AccessibilityLeftEditor, ViewModel.Property.Name);

			YEditor.AccessibilityEnabled = YEditor.Enabled;
			YEditor.AccessibilityTitle = string.Format (Properties.Resources.AccessibilityTopEditor, ViewModel.Property.Name);

			WidthEditor.AccessibilityEnabled = WidthEditor.Enabled;
			WidthEditor.AccessibilityTitle = string.Format (Properties.Resources.AccessibilityRightEditor, ViewModel.Property.Name);

			HeightEditor.AccessibilityEnabled = HeightEditor.Enabled;
			HeightEditor.AccessibilityTitle = string.Format (Properties.Resources.AccessibilityBottomEditor, ViewModel.Property.Name);
		}

		protected override void UpdateValue ()
		{
			XEditor.Value = ViewModel.Value.Left;
			YEditor.Value = ViewModel.Value.Top;
			WidthEditor.Value = ViewModel.Value.Right;
			HeightEditor.Value = ViewModel.Value.Bottom;
		}
	}
}
