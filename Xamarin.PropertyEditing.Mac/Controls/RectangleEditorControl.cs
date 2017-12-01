using System;
using System.Drawing;
using AppKit;
using CoreGraphics;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.Mac
{
	internal abstract class RectangleEditorControl<T>: BaseRectangleEditorControl<T>
		where T : struct
	{
		public RectangleEditorControl ()
		{
			// TODO localize
			XLabel.Frame = new CGRect (29, 23, 25, 24);
			XLabel.StringValue = "X:";

			XEditor.Frame = new CGRect (48, 23, 50, 20);

			YLabel.Frame = new CGRect (155, 23, 25, 24);
			YLabel.StringValue = "Y:";

			YEditor.Frame = new CGRect (175, 23, 50, 20);

			WidthLabel.Frame = new CGRect (3, 0, 45, 24);
			WidthLabel.StringValue = "Width:";

			WidthEditor.Frame = new CGRect (48, 0, 50, 20);

			HeightLabel.Frame = new CGRect (125, 0, 45, 24);
			HeightLabel.StringValue = "Height:";

			HeightEditor.Frame = new CGRect (175, 0, 50, 20);

			RowHeight = 48;
		}
	}

	internal class SystemRectangleEditorControl : RectangleEditorControl<Rectangle>
	{
		protected override void UpdateValue ()
		{
			XEditor.Value = ViewModel.Value.X;
			YEditor.Value = ViewModel.Value.Y;
			WidthEditor.Value = ViewModel.Value.Width;
			HeightEditor.Value = ViewModel.Value.Height;
		}
	}

	internal class CommonRectangleEditorControl : RectangleEditorControl<CommonRectangle>
	{
		protected override void UpdateValue ()
		{
			XEditor.Value = ViewModel.Value.X;
			YEditor.Value = ViewModel.Value.Y;
			WidthEditor.Value = ViewModel.Value.Width;
			HeightEditor.Value = ViewModel.Value.Height;
		}
	}
}