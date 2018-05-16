using System;
using AppKit;
using CoreAnimation;
using CoreGraphics;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.Mac
{
	class CommonBrushLayer : CALayer
	{
		public CommonBrushLayer ()
		{
			this.CornerRadius = 3;
			this.BorderColor = new CGColor (.5f, .5f, .5f, .5f);
			this.BorderWidth = 1;
			MasksToBounds = true;
		}

		CALayer brushLayer;
		CALayer BrushLayer {
			get => brushLayer;
			set {
				if (brushLayer != null)
					brushLayer.RemoveFromSuperLayer ();

				brushLayer = value;

				if (brushLayer != null)
					AddSublayer (brushLayer);
			}
		}

		CommonBrush brush;
		public CommonBrush Brush {
			get => brush;
			set {
				brush = value;
				BrushLayer = CreateBrushLayer (brush);
				Opacity = brush == null ? 0 : 1;
			}
		}

		public static CALayer CreateBrushLayer (CommonBrush brush)
		{
			switch (brush) {
				case CommonSolidBrush solid:
					return new CALayer {
						BackgroundColor = solid.Color.ToCGColor (),
						Opacity = (float)solid.Opacity
					};
				case CommonGradientBrush gradient:
					return new CommonGradientBrushLayer {
						Opacity = (float)gradient.Opacity
					};
				default:
					return new CALayer {
						BackgroundColor = NSColor.Clear.CGColor
					};
			}
		}

		public override void LayoutSublayers ()
		{
			base.LayoutSublayers ();
			BrushLayer.Frame = Bounds;
			Contents = DrawingExtensions.GenerateCheckerboard (Bounds);
		}

		public NSImage RenderPreview ()
		{
			var scale = this.ContentsScale;
			nint h = (nint)(this.Bounds.Height * scale);
			nint w = (nint)(this.Bounds.Width * scale);
			nint bytesPerRow = w * 4;

			if (h <= 0 || w <= 0)
				return null;

			using (var colorSpace = CGColorSpace.CreateGenericRgb ())
			using (var context = new CGBitmapContext (IntPtr.Zero, w, h, 8, bytesPerRow, colorSpace, CGImageAlphaInfo.PremultipliedLast)) {
				this.RenderInContext (context);
				using (var image = context.ToImage ()) {
					return new NSImage (image, new CGSize (w, h));
				}
			}
		}
	}
}
