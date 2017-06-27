﻿using System;
using CoreGraphics;

namespace Xamarin.PropertyEditing.Mac
{
	internal class CGRectEditorControl
		: BaseRectangleEditorControl<CGRect>
	{
		public CGRectEditorControl ()
		{
			// TODO localize
			XLabel.Frame = new CGRect (22, 20, 25, 20);
			XLabel.Value = "X:";

			XEditor.Frame = new CGRect (45, 25, 50, 20);

			YLabel.Frame = new CGRect (130, 20, 25, 20);
			YLabel.Value = "Y:";

			YEditor.Frame = new CGRect (150, 25, 50, 20);

			WidthLabel.Frame = new CGRect (0, -5, 45, 20);
			WidthLabel.Value = "Width:";

			WidthEditor.Frame = new CGRect (45, 0, 50, 20);

			HeightLabel.Frame = new CGRect (102, -5, 50, 20);
			HeightLabel.Value = "Height:";

			HeightEditor.Frame = new CGRect (150, 0, 50, 20);
		}

		protected override void HandlePropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof (ViewModel.Value)) {
				UpdateModelValue ();
			}
		}

		protected override void OnInputUpdated (object sender, EventArgs e)
		{
			ViewModel.Value = new CGRect (XEditor.FloatValue, YEditor.FloatValue, WidthEditor.FloatValue, HeightEditor.FloatValue);
		}

		protected override void UpdateModelValue ()
		{
			XEditor.StringValue = ViewModel.Value.X.ToString ();
			YEditor.StringValue = ViewModel.Value.Y.ToString ();
			WidthEditor.StringValue = ViewModel.Value.Width.ToString ();
			HeightEditor.StringValue = ViewModel.Value.Height.ToString ();

			ViewModel.RowHeight = 48;
		}
	}
}