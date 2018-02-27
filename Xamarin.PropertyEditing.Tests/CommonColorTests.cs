using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.Tests
{
	public class CommonColorTests
	{
		public static readonly Dictionary<string, ColorTestCase> PrimaryTestCases = new Dictionary<string, ColorTestCase> {
			// { name,                       r    g    b   c%   m%   y%   k%    h°   l%   s%   b%  a}
			{ "Black",   new ColorTestCase(  0,   0,   0,   0,   0,   0, 100,    0,   0,   0,   0) },
			{ "White",   new ColorTestCase(255, 255, 255,   0,   0,   0,   0,    0, 100,   0, 100) },
			{ "Gray",    new ColorTestCase(127, 127, 127,   0,   0,   0,  50,    0,  50,   0,  50) },
			{ "Silver",  new ColorTestCase(191, 191, 191,   0,   0,   0,  25,    0,  75,   0,  75) },
			{ "Red",     new ColorTestCase(255,   0,   0,   0, 100, 100,   0,    0,  50, 100, 100) },
			{ "Lime",    new ColorTestCase(  0, 255,   0, 100,   0, 100,   0,  120,  50, 100, 100) },
			{ "Blue",    new ColorTestCase(  0,   0, 255, 100, 100,   0,   0,  240,  50, 100, 100) },
			{ "Yellow",  new ColorTestCase(255, 255,   0,   0,   0, 100,   0,   60,  50, 100, 100) },
			{ "Cyan",    new ColorTestCase(  0, 255, 255, 100,   0,   0,   0,  180,  50, 100, 100) },
			{ "Magenta", new ColorTestCase(255,   0, 255,   0, 100,   0,   0,  300,  50, 100, 100) },
			{ "Maroon",  new ColorTestCase(127,   0,   0,   0, 100, 100,  50,    0,  25, 100,  50) },
			{ "Green",   new ColorTestCase(  0, 127,   0, 100,   0, 100,  50,  120,  25, 100,  50) },
			{ "Navy",    new ColorTestCase(  0,   0, 127, 100, 100,   0,  50,  240,  25, 100,  50) },
			{ "Olive",   new ColorTestCase(127, 127,   0,   0,   0, 100,  50,   60,  25, 100,  50) },
			{ "Purple",  new ColorTestCase(127,   0, 127,   0, 100,   0,  50,  300,  25, 100,  50) },
			{ "Teal",    new ColorTestCase(  0, 127, 127, 100,   0,   0,  50,  180,  25, 100,  50) },
		};

		public static readonly Dictionary<string, ColorTestCase> RGBTestCases = new Dictionary<string, ColorTestCase> {
			// { name,                                 r    g    b   c%    m%    y%   k%    h°   l%    s%   b%  a}
			{ "Orange",            new ColorTestCase(255, 164,   0,   0, 35.7,  100,   0, 38.6,  50,  100, 100) },
			{ "MediumSpringGreen", new ColorTestCase(  0, 250, 154, 100,    0,   38,   2,  157,  49,  100,  98) },
			{ "Chocolate",         new ColorTestCase(209, 117,  52,   0,   44, 75.1,  18, 24.8,  47, 75.1,  82) },
		};

		[Test, TestCaseSource (typeof(CommonColorTests), "HueColorFromColorCases")]
		public CommonColor HueColorFromColorTests (CommonColor color) => color.HueColor;

		private static IEnumerable HueColorFromColorCases() {
			foreach (KeyValuePair<string, ColorTestCase> testCaseKVP in PrimaryTestCases) {
				yield return new TestCaseData (testCaseKVP.Value.Color)
					.Returns (CommonColor.GetHueColorFromHue(testCaseKVP.Value.Color.Hue))
					.SetName ("HueColorFromColor_" + testCaseKVP.Key);
			}
			foreach (KeyValuePair<string, ColorTestCase> testCaseKVP in RGBTestCases) {
				yield return new TestCaseData (testCaseKVP.Value.Color)
					.Returns (CommonColor.GetHueColorFromHue (testCaseKVP.Value.Color.Hue))
					.SetName ("HueColorFromColor_" + testCaseKVP.Key);
			}
		}

		[Test, TestCaseSource (typeof (CommonColorTests), "CMYKFromColorCases")]
		public CMYK CMYKFromColorTest (CommonColor color) => new CMYK(color.C, color.M, color.Y, color.K);

		private static IEnumerable CMYKFromColorCases () {
			foreach (KeyValuePair<string, ColorTestCase> testCaseKVP in PrimaryTestCases) {
				yield return new TestCaseData (testCaseKVP.Value.Color).Returns (testCaseKVP.Value.CMYK).SetName ("CMYKFromColor_" + testCaseKVP.Key);
			}
			foreach (KeyValuePair<string, ColorTestCase> testCaseKVP in RGBTestCases) {
				yield return new TestCaseData (testCaseKVP.Value.Color).Returns (testCaseKVP.Value.CMYK).SetName ("CMYKFromColor_" + testCaseKVP.Key);
			}
		}

		[Test, TestCaseSource (typeof (CommonColorTests), "ColorFromCMYKCases")]
		public CommonColor ColorFromCMYKTest (CMYK cmyk) => CommonColor.FromCMYK (cmyk.C, cmyk.M, cmyk.Y, cmyk.K);

		private static IEnumerable ColorFromCMYKCases ()
		{
			foreach (KeyValuePair<string, ColorTestCase> testCaseKVP in PrimaryTestCases) {
				yield return new TestCaseData (testCaseKVP.Value.CMYK).Returns (testCaseKVP.Value.Color).SetName ("ColorFromCMYK_" + testCaseKVP.Key);
			}
		}

		[Test, TestCaseSource (typeof (CommonColorTests), "HLSFromColorCases")]
		public HLS HLSFromColorTest (CommonColor color) => new HLS (color.Hue, color.Lightness, color.Saturation);

		private static IEnumerable HLSFromColorCases()
		{
			foreach(KeyValuePair<string, ColorTestCase> testCaseKVP in PrimaryTestCases) {
				yield return new TestCaseData (testCaseKVP.Value.Color).Returns (testCaseKVP.Value.HLS).SetName ("HLSFromColor_" + testCaseKVP.Key);
			}
			foreach (KeyValuePair<string, ColorTestCase> testCaseKVP in RGBTestCases) {
				yield return new TestCaseData (testCaseKVP.Value.Color).Returns (testCaseKVP.Value.HLS).SetName ("HLSFromColor_" + testCaseKVP.Key);
			}
		}

		[Test, TestCaseSource (typeof (CommonColorTests), "ColorFromHLSCases")]
		public CommonColor ColorFromHLSTest (HLS hls) => CommonColor.FromHLS (hls.Hue, hls.Lightness, hls.Saturation);

		private static IEnumerable ColorFromHLSCases ()
		{
			foreach (KeyValuePair<string, ColorTestCase> testCaseKVP in PrimaryTestCases) {
				yield return new TestCaseData (testCaseKVP.Value.HLS).Returns (testCaseKVP.Value.Color).SetName ("ColorFromHLS_" + testCaseKVP.Key);
			}
		}

		[Test, TestCaseSource (typeof (CommonColorTests), "HLSRoundTripCases")]
		public HLS HLSRoundTripTest (HLS hls)
		{
			var color = CommonColor.FromHLS (hls.Hue, hls.Lightness, hls.Saturation);
			return new HLS (color.Hue, color.Lightness, color.Saturation);
		}

		private static IEnumerable HLSRoundTripCases ()
		{
			foreach (KeyValuePair<string, ColorTestCase> testCaseKVP in PrimaryTestCases) {
				yield return new TestCaseData (testCaseKVP.Value.HLS).Returns (testCaseKVP.Value.HLS).SetName ("HLSRoundTrip_" + testCaseKVP.Key);
			}
		}

		[Test, TestCaseSource (typeof (CommonColorTests), "HSBFromColorCases")]
		public HSB HSBFromColorTest (CommonColor color) => new HSB (color.Hue, color.Saturation, color.Brightness);

		private static IEnumerable HSBFromColorCases ()
		{
			foreach (KeyValuePair<string, ColorTestCase> testCaseKVP in PrimaryTestCases) {
				yield return new TestCaseData (testCaseKVP.Value.Color).Returns (testCaseKVP.Value.HSB).SetName ("HSBFromColor_" + testCaseKVP.Key);
			}
			foreach (KeyValuePair<string, ColorTestCase> testCaseKVP in RGBTestCases) {
				yield return new TestCaseData (testCaseKVP.Value.Color).Returns (testCaseKVP.Value.HSB).SetName ("HSBFromColor_" + testCaseKVP.Key);
			}
		}

		[Test, TestCaseSource (typeof (CommonColorTests), "ColorFromHSBCases")]
		public CommonColor ColorFromHSBTest (HSB hsb) => CommonColor.FromHSB (hsb.Hue, hsb.Saturation, hsb.Brightness);

		private static IEnumerable ColorFromHSBCases ()
		{
			foreach (KeyValuePair<string, ColorTestCase> testCaseKVP in PrimaryTestCases) {
				yield return new TestCaseData (testCaseKVP.Value.HSB).Returns (testCaseKVP.Value.Color).SetName ("ColorFromHSB_" + testCaseKVP.Key);
			}
		}

		[Test, TestCaseSource (typeof (CommonColorTests), "HSBRoundTripCases")]
		public HSB HSBRoundTripTest (HSB hsb)
		{
			var color = CommonColor.FromHSB (hsb.Hue, hsb.Saturation, hsb.Brightness);
			return new HSB (color.Hue, color.Saturation, color.Brightness);
		}

		private static IEnumerable HSBRoundTripCases ()
		{
			foreach (KeyValuePair<string, ColorTestCase> testCaseKVP in PrimaryTestCases) {
				yield return new TestCaseData (testCaseKVP.Value.HSB).Returns (testCaseKVP.Value.HSB).SetName ("HSBRoundTrip_" + testCaseKVP.Key);
			}
		}

		[Test]
		public void ColorEquality()
		{
			var c1 = new CommonColor (1, 2, 3, 4, "one");
			var c2 = new CommonColor (1, 2, 3, 4, "two");
			var c3 = new CommonColor (1, 2, 3, 5, "three");
			var c4 = new CommonColor (5, 6, 7, 4, "four");

			Assert.That (c1 == c2, Is.True);
			Assert.That (c1 != c2, Is.False);
			Assert.That (c1.Equals (c2), Is.True);
			Assert.That (c1.Equals (c3), Is.False);
			Assert.That (c1.Equals (c3, true), Is.True);
			Assert.That (c1.Equals (c4), Is.False);
		}

		[Test]
		public void ColorToString()
		{
			var color = new CommonColor (0x34, 0x56, 0x78, 0x12);
			Assert.AreEqual ("#12345678", color.ToString ());
		}

#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
		public struct CMYK
		{
			public CMYK(double c, double m, double y, double k)
			{
				C = c;
				M = m;
				Y = y;
				K = k;
			}

			public double C { get; }
			public double M { get; }
			public double Y { get; }
			public double K { get; }

			public override bool Equals (object obj)
			{
				var other = (CMYK)obj;
				return Math.Round (other.C - C, 2) == 0
					&& Math.Round (other.M - M, 2) == 0
					&& Math.Round (other.Y - Y, 2) == 0
					&& Math.Round (other.K - K, 2) == 0;
			}

			public override string ToString () => $"{{ C:{C:P1}, M:{M:P1}, Y:{Y:P1}, K:{K:P1} }}";
		}

		public struct HLS
		{
			public HLS (double hue, double lightness, double saturation)
			{
				Hue = hue;
				Lightness = lightness;
				Saturation = saturation;
			}

			public double Hue { get; }
			public double Lightness { get; }
			public double Saturation { get; }

			public override bool Equals (object obj)
			{
				var other = (HLS)obj;
				return Math.Round (other.Hue - Hue, 1) == 0
					&& Math.Round (other.Lightness - Lightness, 1) == 0
					&& Math.Round (other.Saturation - Saturation, 1) == 0;
			}

			public override string ToString () => $"{{ H:{Hue:F1}°, S:{Saturation:P1}, L:{Lightness:P1} }}";
		}

		public struct HSB
		{
			public HSB (double hue, double saturation, double brightness)
			{
				Hue = hue;
				Saturation = saturation;
				Brightness = brightness;
			}

			public double Hue { get; }
			public double Saturation { get; }
			public double Brightness { get; }

			public override bool Equals (object obj)
			{
				var other = (HSB)obj;
				return Math.Round (other.Hue - Hue, 1) == 0
					&& Math.Round (other.Saturation - Saturation, 1) == 0
					&& Math.Round (other.Brightness - Brightness, 1) == 0;
			}

			public override string ToString () => $"{{ H:{Hue:F1}°, S:{Saturation:P1}, B:{Brightness:P1} }}";
		}
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()

		public struct ColorTestCase
		{
			public ColorTestCase(
				byte red, byte green, byte blue,
				double cyanPercent, double magentaPercent, double yellowPercent, double blackPercent,
				double hue, double lightnessPercent, double saturationPercent,
				double brightnessPercent, byte alpha = 255)
			{
				Color = new CommonColor (red, green, blue, alpha);
				CMYK = new CMYK (cyanPercent / 100, magentaPercent / 100, yellowPercent / 100, blackPercent / 100);
				HLS = new HLS (hue, lightnessPercent / 100, saturationPercent/ 100);
				HSB = new HSB (hue, saturationPercent / 100, brightnessPercent / 100);
			}
			public CommonColor Color { get; }
			public CMYK CMYK { get; }
			public HLS HLS { get; }
			public HSB HSB { get; }
		}
	}
}
