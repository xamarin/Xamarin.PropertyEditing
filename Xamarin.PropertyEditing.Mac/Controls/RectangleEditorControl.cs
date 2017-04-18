using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using AppKit;
using CoreGraphics;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class RectangleEditorControl<T> : BaseRectangleEditorControl<T>
		where T : struct
	{
		public RectangleEditorControl ()
		{
			XLabel.Frame = new CGRect (0, -5, 25, 20);
			XLabel.Value = "X:";

			XEditor.Frame = new CGRect (25, 0, 50, 20);

			YLabel.Frame = new CGRect (85, -5, 25, 20);
			YLabel.Value = "Y:";

			YEditor.Frame = new CGRect (105, 0, 50, 20);

			WidthLabel.Frame = new CGRect (160, -5, 45, 20);
			WidthLabel.Value = "Width:";

			WidthEditor.Frame = new CGRect (205, 0, 50, 20);

			HeightLabel.Frame = new CGRect (260, -5, 45, 20);
			HeightLabel.Value = "Height:";

			HeightEditor.Frame = new CGRect (305, 0, 50, 20);
		}

		protected override void UpdateModelValue ()
		{
			var rectType = ViewModel.Value.GetType ();
			var xProp = rectType.GetProperty ("X");
			var xValue = (nfloat)xProp.GetValue (ViewModel.Value);
			var yProp = rectType.GetProperty ("Y");
			var yValue = (nfloat)yProp.GetValue (ViewModel.Value);
			var wProp = rectType.GetProperty ("Width");
			var wValue = (nfloat)wProp.GetValue (ViewModel.Value);
			var hProp = rectType.GetProperty ("Height");
			var hValue = (nfloat)hProp.GetValue (ViewModel.Value);

			CGRect rect = new CGRect (xValue, yValue, wValue, hValue);
			XEditor.StringValue = rect.X.ToString ();
			YEditor.StringValue = rect.Y.ToString ();
			WidthEditor.StringValue = rect.Width.ToString ();
			HeightEditor.StringValue = rect.Height.ToString ();
		}
	}
}