using System;
using System.Drawing;
using AppKit;
using CoreGraphics;

namespace Xamarin.PropertyEditing.Mac
{
	internal class RectangleEditorControl
		: BaseRectangleEditorControl<Rectangle>
	{
		public RectangleEditorControl ()
		{
			XLabel.Frame = new CGRect (26, 23, 25, 24);
			XLabel.StringValue = "X:";

			XEditor.Frame = new CGRect (45, 23, 50, 20);

			YLabel.Frame = new CGRect (135, 23, 25, 24);
			YLabel.StringValue = "Y:";

			YEditor.Frame = new CGRect (150, 23, 50, 20);

			WidthLabel.Frame = new CGRect (0, 0, 45, 24);
			WidthLabel.StringValue = "Width:";

			WidthEditor.Frame = new CGRect (45, 0, 50, 20);

			HeightLabel.Frame = new CGRect (105, 0, 45, 24);
			HeightLabel.StringValue = "Height:";

			HeightEditor.Frame = new CGRect (150, 0, 50, 20);

			RowHeight = 48;
		}

		protected override void OnInputUpdated (object sender, EventArgs e)
		{
			ViewModel.Value = new Rectangle ((int)XEditor.Value, (int)YEditor.Value, (int)WidthEditor.Value, (int)HeightEditor.Value);
		}

		protected override void UpdateValue ()
		{
			XEditor.Value = ViewModel.Value.X;
			YEditor.Value = ViewModel.Value.Y;
			WidthEditor.Value = ViewModel.Value.Width;
			HeightEditor.Value = ViewModel.Value.Height;
		}
	}
}