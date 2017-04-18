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
			XLabel.Frame = new CGRect (0, -5, 25, 20);
			XLabel.Value = "Width:";

			XEditor.Frame = new CGRect (25, 0, 50, 20);

			YLabel.Frame = new CGRect (85, -5, 25, 20);
			YLabel.Value = "Height:";

			YEditor.Frame = new CGRect (110, 0, 50, 20);
		}

		protected override void UpdateModelValue ()
		{
			XEditor.StringValue = ViewModel.Value.Width.ToString ();
			YEditor.StringValue = ViewModel.Value.Width.ToString ();
		}
	}
}
