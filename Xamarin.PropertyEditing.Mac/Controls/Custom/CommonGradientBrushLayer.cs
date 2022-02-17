using System;
using System.Linq;
using CoreAnimation;
using CoreGraphics;
using ObjCRuntime;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.Mac
{
	internal class CommonGradientBrushLayer : CALayer
	{
		public CommonGradientBrushLayer ()
		{
		}

		public CommonGradientBrushLayer (NativeHandle handle) : base (handle)
		{
		}

		private CommonGradientBrush brush;
		public CommonGradientBrush Brush {
			get => brush;
			set {
				brush = value;
				SetNeedsDisplay ();
			}
		}

		public override void DrawInContext (CGContext ctx)
		{
			ctx.SaveState ();

			var colorspace = CGColorSpace.CreateDeviceRGB ();
			var colors = Brush.GradientStops.Select (stop => stop.Color.ToCGColor ()).ToArray ();
			var locations = Brush.GradientStops.Select (stop => (nfloat)stop.Offset).ToArray ();

			var gradient = new CGGradient (colorspace, colors, locations);
			var center = new CGPoint (Bounds.Width / 2f, Bounds.Height / 2f);
			var radius = (float)Math.Min (Bounds.Width / 2.0, Bounds.Height / 2.0);

			switch (Brush) {
				case CommonLinearGradientBrush linear:
					ctx.DrawLinearGradient (
						gradient,
						new CGPoint (0, 0),
						new CGPoint (0, Bounds.Width),
						CGGradientDrawingOptions.None);
					break;
				case CommonRadialGradientBrush radial:
					ctx.DrawRadialGradient (
						gradient,
						startCenter: center,
						startRadius: 0f,
						endCenter: center,
						endRadius: radius,
						options: CGGradientDrawingOptions.None);
					break;
			}
		}
	}
}
