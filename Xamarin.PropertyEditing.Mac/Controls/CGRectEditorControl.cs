using System;
using CoreGraphics;

namespace Xamarin.PropertyEditing.Mac
{
	internal class CGRectEditorControl
		: BaseRectangleEditorControl<CGRect>
	{
		public CGRectEditorControl ()
		{
			// TODO localize
			XLabel.Frame = new CGRect (19, 23, 25, 24);
			XLabel.StringValue = "X:";

			XEditor.Frame = new CGRect (38, 23, 50, 20);

			YLabel.Frame = new CGRect (130, 23, 25, 24);
			YLabel.StringValue = "Y:";

			YEditor.Frame = new CGRect (145, 23, 50, 20);

			WidthLabel.Frame = new CGRect (5, 0, 45, 24);
			WidthLabel.StringValue = "Width:";

			WidthEditor.Frame = new CGRect (40, 0, 50, 20);

			HeightLabel.Frame = new CGRect (125, 0, 45, 24);
			HeightLabel.StringValue = "Height:";

			HeightEditor.Frame = new CGRect (175, 0, 50, 20);

			this.DoConstraints (new[] {
				XEditor.ConstraintTo (this, (xe, c) => xe.Width == 50),
				YEditor.ConstraintTo (this, (ye, c) => ye.Width == 50),
				WidthEditor.ConstraintTo (this, (we, c) => we.Width == 50),
				HeightEditor.ConstraintTo (this, (he, c) => he.Width == 50),
			});

			RowHeight = 48;
		}

		protected override void OnInputUpdated (object sender, EventArgs e)
		{
			ViewModel.Value = new CGRect (XEditor.Value, YEditor.Value, WidthEditor.Value, HeightEditor.Value);
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