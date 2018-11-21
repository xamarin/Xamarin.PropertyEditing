using System;
using AppKit;
using CoreGraphics;
using Xamarin.PropertyEditing.Drawing;
using Xamarin.PropertyEditing.Mac.Resources;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class CommonThicknessEditorControl : BaseRectangleEditorControl<CommonThickness>
	{
		public CommonThicknessEditorControl ()
		{
			XLabel.Frame = new CGRect (28, 23, 50, 22);
			XLabel.Font = NSFont.FromFontName (DefaultFontName, DefaultDescriptionLabelFontSize);
			XLabel.StringValue = "LEFT";

			XEditor.Frame = new CGRect (-1, 46, 90, 20);

			YLabel.Frame = new CGRect (160, 23, 45, 22);
			YLabel.Font = NSFont.FromFontName (DefaultFontName, DefaultDescriptionLabelFontSize);
			YLabel.StringValue = "TOP";

			YEditor.Frame = new CGRect (132, 46, 90, 20);

			WidthLabel.Frame = new CGRect (24, -10, 50, 22);
			WidthLabel.Font = NSFont.FromFontName (DefaultFontName, DefaultDescriptionLabelFontSize);
			WidthLabel.StringValue = "RIGHT";

			WidthEditor.Frame = new CGRect (-1, 13, 90, 20);

			HeightLabel.Frame = new CGRect (150, -10, 50, 22);
			HeightLabel.Font = NSFont.FromFontName (DefaultFontName, DefaultDescriptionLabelFontSize);
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
			XEditor.AccessibilityTitle = string.Format (LocalizationResources.AccessibilityLeftEditor, ViewModel.Property.Name);

			YEditor.AccessibilityEnabled = YEditor.Enabled;
			YEditor.AccessibilityTitle = string.Format (LocalizationResources.AccessibilityTopEditor, ViewModel.Property.Name);

			WidthEditor.AccessibilityEnabled = WidthEditor.Enabled;
			WidthEditor.AccessibilityTitle = string.Format (LocalizationResources.AccessibilityRightEditor, ViewModel.Property.Name);

			HeightEditor.AccessibilityEnabled = HeightEditor.Enabled;
			HeightEditor.AccessibilityTitle = string.Format (LocalizationResources.AccessibilityBottomEditor, ViewModel.Property.Name);
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
