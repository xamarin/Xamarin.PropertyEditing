using System;
using System.Drawing;
using AppKit;
using CoreGraphics;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.Mac
{
	internal abstract class RectangleEditorControl<T>: BaseRectangleEditorControl<T>
	{
		public RectangleEditorControl ()
		{
			// TODO localize
			XLabel.Frame = new CGRect (38, 27, 25, 22);
			XLabel.Font = NSFont.FromFontName (DefaultFontName, DefaultDescriptionLabelFontSize); // TODO: Washed-out color following specs
			XLabel.StringValue = "X";

			XEditor.Frame = new CGRect (4, 46, 90, 20);

			YLabel.Frame = new CGRect (166, 27, 25, 22);
			YLabel.Font = NSFont.FromFontName (DefaultFontName, DefaultDescriptionLabelFontSize); // TODO: Washed-out color following specs
			YLabel.StringValue = "Y";

			YEditor.Frame = new CGRect (132, 46, 90, 20);

			WidthLabel.Frame = new CGRect (24, -6, 50, 22);
			WidthLabel.Font = NSFont.FromFontName (DefaultFontName, DefaultDescriptionLabelFontSize); // TODO: Washed-out color following specs
			WidthLabel.StringValue = "WIDTH";

			WidthEditor.Frame = new CGRect (4, 13, 90, 20);

			HeightLabel.Frame = new CGRect (150, -6, 50, 22);
			HeightLabel.Font = NSFont.FromFontName (DefaultFontName, DefaultDescriptionLabelFontSize); // TODO: Washed-out color following specs
			HeightLabel.StringValue = "HEIGHT";

			HeightEditor.Frame = new CGRect (132, 13, 90, 20);

			RowHeight = 66;
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