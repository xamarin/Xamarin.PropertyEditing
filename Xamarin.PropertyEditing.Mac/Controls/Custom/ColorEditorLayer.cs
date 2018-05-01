using System;
using CoreAnimation;
using CoreGraphics;

namespace Xamarin.PropertyEditing.Mac
{
	abstract class ColorEditorLayer : CALayer
	{
		public ColorEditorLayer ()
		{
		}

		public ColorEditorLayer (IntPtr handle) : base (handle)
		{
		}

		abstract public void UpdateFromModel (EditorInteraction viewModel);
		abstract public void UpdateFromLocation (EditorInteraction viewModel, CGPoint location);
	}
}
