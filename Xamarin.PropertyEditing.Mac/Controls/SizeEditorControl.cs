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
	internal class SizeEditorControl : BasePointEditorControl<CGSize>
	{
		public SizeEditorControl ()
		{
			XLabel.Frame = new CGRect (0, -5, 45, 20);
			XLabel.Value = "Width:";

			XEditor.Frame = new CGRect (50, 0, 50, 20);
			XEditor.Activated += (sender, e) => {
				ViewModel.Value = new CGSize (XEditor.FloatValue, YEditor.FloatValue);
			};

			YLabel.Frame = new CGRect (105, -5, 50, 20);
			YLabel.Value = "Height:";

			YEditor.Frame = new CGRect (160, 0, 50, 20);
			YEditor.Activated += (sender, e) => {
				ViewModel.Value = new CGSize (XEditor.FloatValue, YEditor.FloatValue);
			};
		}

		protected override void UpdateModelValue ()
		{
			XEditor.StringValue = ViewModel.Value.Width.ToString ();
			YEditor.StringValue = ViewModel.Value.Height.ToString ();
		}
	}
}
