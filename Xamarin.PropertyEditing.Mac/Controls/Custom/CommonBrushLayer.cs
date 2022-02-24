using System;
using AppKit;
using CoreAnimation;
using CoreGraphics;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.Mac
{
	internal class CommonBrushLayer
		: CALayer
	{
		private const double VerticalMarginOffset = 0.5;
		private CGRect frameRect;

		public CommonBrushLayer (IHostResourceProvider hostResources)
		{
			if (hostResources == null)
				throw new ArgumentNullException (nameof (hostResources));

			this.hostResources = hostResources;

			CornerRadius = 3;
			BorderColor = new CGColor (.5f, .5f, .5f, .5f);
			BorderWidth = 1;
			MasksToBounds = true;
		}

		internal uint VerticalMargin { get; set; }

		private CALayer brushLayer;
		private CALayer BrushLayer {
			get => brushLayer;
			set {
				if (brushLayer != null)
					brushLayer.RemoveFromSuperLayer ();

				brushLayer = value;

				if (brushLayer != null)
					AddSublayer (brushLayer);
			}
		}

		private CommonBrush brush;
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

			if (frameRect.IsEmpty)
				frameRect = new CGRect (Frame.X, Bounds.Y + VerticalMargin + VerticalMarginOffset, Bounds.Width, Bounds.Height - (VerticalMargin * 2));
			Frame = frameRect;
			BrushLayer.Frame = Bounds;
			Contents = DrawingExtensions.GenerateCheckerboard (Bounds, this.hostResources.GetNamedColor (NamedResources.Checkerboard0Color), this.hostResources.GetNamedColor (NamedResources.Checkerboard1Color));
		}

		public NSImage RenderPreview ()
		{
			var scale = this.ContentsScale;
			// The double cast below is needed for now, while Width/Height/scale are of type ObjCRuntime.nfloat
			// in order for the floating point to integral conversion to work properly. Later when ObjCRuntime.nfloat
			// is replaced with System.Runtime.InteropServices.NFloat that won't be needed (but can't hurt).
			nint h = (nint)(double)(this.Bounds.Height * scale);
			nint w = (nint)(double)(this.Bounds.Width * scale);
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

		private readonly IHostResourceProvider hostResources;
	}
}
