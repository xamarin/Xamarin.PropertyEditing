using System;
using AppKit;
using CoreGraphics;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.Mac
{
	public class CommonBrushView : NSView
	{
		public CommonBrush Brush {
			get => (Layer as CommonBrushLayer)?.Brush;
			set => (Layer as CommonBrushLayer).Brush = value;
		}

		public CommonBrushView ()
		{
			Initialize ();
		}

		public CommonBrushView (CGRect frame) : base (frame)
		{
			Initialize ();
		}

		public CommonBrushView (IntPtr handle) : base (handle)
		{
		}

		void Initialize () {
			WantsLayer = true;
			Layer = new CommonBrushLayer
			{
				Brush = Brush
			};
		}
    }
}
