using System;
using System.Drawing;
using AppKit;
using CoreGraphics;
using Xamarin.PropertyEditing.Drawing;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal abstract class RectangleEditorControl<T>: BaseRectangleEditorControl<T>
	{
		public RectangleEditorControl ()
		{
			// TODO localize
			XLabel.Frame = new CGRect (34, 23, 25, 22);
			XLabel.Font = NSFont.FromFontName (DefaultFontName, DefaultDescriptionLabelFontSize); // TODO: Washed-out color following specs
			XLabel.StringValue = "X";

			XEditor.Frame = new CGRect (-1, 46, 90, 20);

			YLabel.Frame = new CGRect (166, 23, 25, 22);
			YLabel.Font = NSFont.FromFontName (DefaultFontName, DefaultDescriptionLabelFontSize); // TODO: Washed-out color following specs
			YLabel.StringValue = "Y";

			YEditor.Frame = new CGRect (132, 46, 90, 20);

			WidthLabel.Frame = new CGRect (20, -10, 50, 22);
			WidthLabel.Font = NSFont.FromFontName (DefaultFontName, DefaultDescriptionLabelFontSize); // TODO: Washed-out color following specs
			WidthLabel.StringValue = "WIDTH";

			WidthEditor.Frame = new CGRect (-1, 13, 90, 20);

			HeightLabel.Frame = new CGRect (150, -10, 50, 22);
			HeightLabel.Font = NSFont.FromFontName (DefaultFontName, DefaultDescriptionLabelFontSize); // TODO: Washed-out color following specs
			HeightLabel.StringValue = "HEIGHT";

			HeightEditor.Frame = new CGRect (132, 13, 90, 20);
		}

		public override nint GetHeight (EditorViewModel vm)
		{
			return 66;
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