using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using AppKit;
using CoreGraphics;
using Foundation;
using Xamarin.PropertyEditing.Mac.Resources;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class CGPointEditorControl : PointEditorControl<CGPoint>
	{
		protected override void OnInputUpdated (object sender, EventArgs e)
		{
			ViewModel.Value = new CGPoint (XEditor.Value, YEditor.Value);
		}

		protected override void UpdateValue ()
		{
			XEditor.Value = ViewModel.Value.X;
			YEditor.Value = ViewModel.Value.Y;
		}
	}
}
