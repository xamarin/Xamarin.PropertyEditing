using System;
using AppKit;
using CoreGraphics;
using CoreImage;
using ObjCRuntime;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.Mac
{
	static class DrawingExtensions
	{
		public static CGImage GenerateCheckerboard (CGRect frame, NSColor c0, NSColor c1)
		{
			return GenerateCheckerboard (frame, CIColor.FromCGColor (c0.CGColor), CIColor.FromCGColor (c1.CGColor));
		}

		public static CGImage GenerateCheckerboard (CGRect frame, CIColor c0, CIColor c1)
		{
			using (var board = new CICheckerboardGenerator () {
				Color0 = c0,
				Color1 = c1,
				Width = (float)Math.Min (frame.Height / 2f, 10),
				InputCenter = default,
			}) {
				using (var context = new CIContext (null)) {
					return context.CreateCGImage (board.OutputImage, new CGRect (0, 0, frame.Width, frame.Height));
				}
			}
		}

		public static CGColor ToCGColor (this CommonColor color)
			=> new CGColor (color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);

		public static NSColor ToNSColor (this CommonColor color)
			=> NSColor.FromRgba (color.R, color.G, color.B, color.A);

		public static CommonColor Blend (this CommonColor a, CommonColor b)
		{
			byte C (byte cb1, byte ab1, byte cb2)
			{
				var c1 = cb1 / 255f;
				var a1 = ab1 / 255f;
				var c2 = cb2 / 255f;

				var c = Math.Max (0, Math.Min (255, (c1 + c2 * (1 - a1)) * 255));
				return (byte)c;
			}

			return new CommonColor (
				C (a.R, a.A, b.R),
				C (a.G, a.A, b.G),
				C (a.B, a.A, b.B),
				C (a.A, a.A, b.A));
		}

		public static CGRect Translate (this CGRect rect, double x, double y)
			=> new CGRect (
				x: rect.X + x,
				y: rect.Y + y,
				width: rect.Width,
				height: rect.Height);

		public static CommonColor UpdateRGB (
			this CommonColor color,
			byte? r = null,
			byte? g = null,
			byte? b = null,
			byte? a = null)
		{
			return new CommonColor (
				r: r ?? color.R,
				g: g ?? color.G,
				b: b ?? color.B,
				a: a ?? color.A);
		}

		public static CommonColor UpdateHSB (
			this CommonColor color,
			double? hue = null,
			double? saturation = null,
			double? brightness = null,
			byte? alpha = null)
		{
			return CommonColor.FromHSB (
				hue: hue ?? color.Hue,
				saturation: saturation ?? color.Saturation,
				brightness: brightness ?? color.Brightness,
				alpha: alpha ?? color.A);
		}

		public static CommonColor UpdateHLS (
			this CommonColor color,
			double? hue = null,
			double? lightness = null,
			double? saturation = null,
			byte? alpha = null)
		{
			return CommonColor.FromHLS (
				hue: hue ?? color.Hue,
				lightness: lightness ?? color.Lightness,
				saturation: saturation ?? color.Saturation,
				alpha: alpha ?? color.A);
		}

		public static CommonColor UpdateCMYK (
			this CommonColor color,
			double? c = null,
			double? m = null,
			double? y = null,
			double? k = null,
			byte? alpha = null)
		{
			return CommonColor.FromCMYK (
				c: c ?? color.C,
				m: m ?? color.M,
				y: y ?? color.Y,
				k: k ?? color.K,
				alpha: alpha ?? color.A);
		}
	}
}
