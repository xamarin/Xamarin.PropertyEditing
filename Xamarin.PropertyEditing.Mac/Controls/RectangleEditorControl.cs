using System;
using System.Drawing;
using AppKit;
using CoreGraphics;
using Foundation;
using Xamarin.PropertyEditing.Drawing;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal abstract class RectangleEditorControl<T>
		: BaseRectangleEditorControl<T>
	{
		protected RectangleEditorControl (IHostResourceProvider hostResources)
			: base (hostResources)
		{
			// TODO localize
			XLabel.StringValue = "X";

			XEditor.Frame = new CGRect (0, 46, 90, 20);
			
			YLabel.StringValue = "Y";

			YEditor.Frame = new CGRect (132, 46, 90, 20);
			
			WidthLabel.StringValue = "WIDTH";

			WidthEditor.Frame = new CGRect (0, 13, 90, 20);
			
			HeightLabel.StringValue = "HEIGHT";

			HeightEditor.Frame = new CGRect (132, 13, 90, 20);
		}

		public override nint GetHeight (EditorViewModel vm)
		{
			return 66;
		}
	}

	internal class SystemRectangleEditorControl
		: RectangleEditorControl<Rectangle>
	{
		public SystemRectangleEditorControl (IHostResourceProvider hostResources)
			: base (hostResources)
		{
		}

		protected override void UpdateValue ()
		{
			XEditor.Value = ViewModel.Value.X;
			YEditor.Value = ViewModel.Value.Y;
			WidthEditor.Value = ViewModel.Value.Width;
			HeightEditor.Value = ViewModel.Value.Height;
		}
	}

	internal class CommonRectangleEditorControl
		: RectangleEditorControl<CommonRectangle>
	{
		public CommonRectangleEditorControl (IHostResourceProvider hostResources)
			: base (hostResources)
		{
		}

		protected override void UpdateValue ()
		{
			XEditor.Value = ViewModel.Value.X;
			YEditor.Value = ViewModel.Value.Y;
			WidthEditor.Value = ViewModel.Value.Width;
			HeightEditor.Value = ViewModel.Value.Height;
		}
	}
}