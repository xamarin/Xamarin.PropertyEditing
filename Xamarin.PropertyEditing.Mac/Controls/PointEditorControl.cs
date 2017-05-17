using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using AppKit;
using CoreGraphics;
using Foundation;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class PointEditorControl : BasePointEditorControl<CGPoint>
	{
		public PointEditorControl ()
		{
			XLabel.Frame = new CGRect (0, -5, 25, 20);
			XLabel.Value = "X:";

			XEditor.Frame = new CGRect (25, 0, 50, 20);
			XEditor.Activated += (sender, e) => {
				ViewModel.Value = new CGPoint (XEditor.FloatValue, YEditor.FloatValue);
			};

			YLabel.Frame = new CGRect (85, -5, 25, 20);
			YLabel.Value = "Y:";

			YEditor.Frame = new CGRect (110, 0, 50, 20);
			YEditor.Activated += (sender, e) => {
				ViewModel.Value = new CGPoint (XEditor.FloatValue, YEditor.FloatValue);
			};
		}

		protected override void UpdateModelValue ()
		{
			XEditor.StringValue = ViewModel.Value.X.ToString ();
			YEditor.StringValue = ViewModel.Value.Y.ToString ();
		}
	}
}
