using System;
using AppKit;
using CoreGraphics;
using CoreImage;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.Mac
{
	public static class DrawingExtensions
	{
		public static NSImage CreateSwatch (this CommonColor color, CGSize size)
		{
			var c0 = CIColor.FromCGColor (color.Blend (CommonColor.White).ToCGColor ());
			var c1 = CIColor.FromCGColor (color.Blend (CommonColor.Black).ToCGColor ());

			return CreateSwatch (color, size, c0, c1);
		}

		public static NSImage CreateSwatch (this CommonColor color, CGSize size, CIColor c0, CIColor c1)
			=> new NSImage (GenerateCheckerboard (new CGRect (0, 0, size.Width,size.Height), c0, c1), size);


		public static CGImage GenerateCheckerboard (CGRect frame)
			=> GenerateCheckerboard (frame, CIColor.WhiteColor, CIColor.BlackColor);

		public static CGImage GenerateCheckerboard (CGRect frame, CIColor c0, CIColor c1)
		{
			using (var board = new CICheckerboardGenerator () {
				Color0 = c0,
				Color1 = c1,
				Width = (float)Math.Min (frame.Height / 2f, 10),
				Center = new CIVector (new nfloat[] { 0, 0 }),
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
	}
}
