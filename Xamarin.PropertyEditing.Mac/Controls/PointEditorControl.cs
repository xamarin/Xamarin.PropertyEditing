using System;
using System.Drawing;
using CoreGraphics;
using Xamarin.PropertyEditing.Drawing;
using Xamarin.PropertyEditing.Mac.Resources;

namespace Xamarin.PropertyEditing.Mac
{
	internal abstract class PointEditorControl<T> : BasePointEditorControl<T>
		where T : struct
	{
		public PointEditorControl ()
		{
			XLabel.Frame = new CGRect (29, 0, 25, 24);
			XLabel.StringValue = "X:"; // TODO Localise

			XEditor.Frame = new CGRect (48, 0, 50, 20);

			YLabel.Frame = new CGRect (155, 0, 25, 24);
			YLabel.StringValue = "Y:"; // TODO Localise

			YEditor.Frame = new CGRect (175, 0, 50, 20);
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
