using System;
using System.Drawing;
using CoreGraphics;

namespace Xamarin.PropertyEditing.Mac
{
	internal class RectangleEditorControl
		: BaseRectangleEditorControl<Rectangle>
	{
		public RectangleEditorControl ()
		{
			// TODO localize
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
			ViewModel.Value = new Rectangle (XEditor.IntValue, YEditor.IntValue, WidthEditor.IntValue, HeightEditor.IntValue);
		}

		protected override void UpdateValue ()
		{
			XEditor.StringValue = ViewModel.Value.X.ToString ();
			YEditor.StringValue = ViewModel.Value.Y.ToString ();
			WidthEditor.StringValue = ViewModel.Value.Width.ToString ();
			HeightEditor.StringValue = ViewModel.Value.Height.ToString ();
		}
	}
}