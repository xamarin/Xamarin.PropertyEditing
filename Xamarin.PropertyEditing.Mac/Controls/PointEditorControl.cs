using System;
using System.Drawing;
using AppKit;
using CoreGraphics;
using Xamarin.PropertyEditing.Drawing;
using Xamarin.PropertyEditing.Mac.Resources;

namespace Xamarin.PropertyEditing.Mac
{
	internal abstract class PointEditorControl<T> : BasePointEditorControl<T>
	{
		public PointEditorControl ()
		{
			XLabel.Frame = new CGRect (38, -6, 25, 22);
			XLabel.Font = NSFont.FromFontName (DefaultFontName, DefaultDescriptionLabelFontSize); // TODO: Washed-out color following specs
			XLabel.StringValue = "X"; // TODO Localise

			XEditor.Frame = new CGRect (4, 13, 90, 20);

			YLabel.Frame = new CGRect (166, -6, 25, 22);
			YLabel.Font = NSFont.FromFontName (DefaultFontName, DefaultDescriptionLabelFontSize); // TODO: Washed-out color following specs
			YLabel.StringValue = "Y"; // TODO Localise

			YEditor.Frame = new CGRect (132, 13, 90, 20);

			RowHeight = 33;
		}
	}

	internal class SystemPointEditorControl : PointEditorControl<Point>
	{
		protected override void UpdateValue ()
		{
			XEditor.Value = ViewModel.Value.X;
			YEditor.Value = ViewModel.Value.Y;
		}
	}

	internal class CommonPointEditorControl : PointEditorControl<CommonPoint>
	{
		protected override void UpdateValue ()
		{
			XEditor.Value = ViewModel.Value.X;
			YEditor.Value = ViewModel.Value.Y;
		}
	}
}
