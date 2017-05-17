using System;
using System.Drawing;
using CoreGraphics;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.Mac
{
	internal abstract class SizeEditorControl<T> : BasePointEditorControl<T>
		where T : struct
	{
		public SizeEditorControl ()
		{
			XLabel.Frame = new CGRect (0, 0, 40, 24);
			XLabel.StringValue = "Width:"; // TODO Localise

			XEditor.Frame = new CGRect (45, 0, 50, 20);

			YLabel.Frame = new CGRect (105, 0, 45, 24);
			YLabel.StringValue = "Height:"; // TODO Localise

			YEditor.Frame = new CGRect (155, 0, 50, 20);
		}

		protected override void UpdateAccessibilityValues ()
		{
			XEditor.AccessibilityTitle = ViewModel.Property.Name + " Width Editor"; // TODO Localization
			YEditor.AccessibilityTitle = ViewModel.Property.Name + " Height Editor"; // TODO Localization
		}
	}

	internal class SystemSizeEditorControl : SizeEditorControl<Size>
	{
		protected override void UpdateValue ()
		{
			XEditor.Value = ViewModel.Value.Width;
			YEditor.Value = ViewModel.Value.Height;
		}

		protected override void OnInputUpdated (object sender, EventArgs e)
		{
			ViewModel.Value = new Size ((int)XEditor.Value, (int)YEditor.Value);
		}
	}

	internal class CommonSizeEditorControl : SizeEditorControl<CommonSize>
	{
		protected override void UpdateValue ()
		{
			XEditor.Value = ViewModel.Value.Width;
			YEditor.Value = ViewModel.Value.Height;
		}

		protected override void OnInputUpdated (object sender, EventArgs e)
		{
			ViewModel.Value = new CommonSize (XEditor.Value, YEditor.Value);
		}
	}
}
