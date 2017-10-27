using System.Collections;
using NUnit.Framework;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.Tests
{
	public class CommonColorTests
	{
		[Test, TestCaseSource (typeof(CommonColorTests), "HueFromColorCases")]
		public CommonColor HueFromColorTests (CommonColor color) => color.HueColor;

		private static IEnumerable HueFromColorCases() {
			// Shades of grey all map to red
			yield return new TestCaseData (new CommonColor (0, 0, 0)).Returns (new CommonColor (255, 0, 0))
				.SetName ("HueFromColorBlackHasRedHue");
			yield return new TestCaseData (new CommonColor (255, 255, 255)).Returns (new CommonColor (255, 0, 0))
				.SetName ("HueFromColorWhiteHasRedHue");
			yield return new TestCaseData (new CommonColor (127, 127, 127)).Returns (new CommonColor (255, 0, 0))
				.SetName ("HueFromColorGreyHasRedHue");
			// Fully-saturated primary colors map to themselves
			yield return new TestCaseData (new CommonColor (255, 0, 0)).Returns (new CommonColor (255, 0, 0))
				.SetName ("HueFromColorRedHueHasRedHue");
			yield return new TestCaseData (new CommonColor (0, 255, 0)).Returns (new CommonColor (0, 255, 0))
				.SetName ("HueFromColorGreenHueHasGreenHue");
			yield return new TestCaseData (new CommonColor (0, 0, 255)).Returns (new CommonColor (0, 0, 255))
				.SetName ("HueFromColorBlueHueHasBlueHue");
			yield return new TestCaseData (new CommonColor (255, 255, 0)).Returns (new CommonColor (255, 255, 0))
				.SetName ("HueFromColorYellowHueHasYellowHue");
			yield return new TestCaseData (new CommonColor (255, 0, 255)).Returns (new CommonColor (255, 0, 255))
				.SetName ("HueFromColorPurpleHueHasPurpleHue");
			yield return new TestCaseData (new CommonColor (0, 255, 255)).Returns (new CommonColor (0, 255, 255))
				.SetName ("HueFromColorCyanHueHasCyanHue");
			// Darker shades of primary colors map to the fully-saturated shade
			yield return new TestCaseData (new CommonColor (127, 0, 0)).Returns (new CommonColor (255, 0, 0))
				.SetName ("HueFromColorDarkRedHasRedHue");
			yield return new TestCaseData (new CommonColor (0, 127, 0)).Returns (new CommonColor (0, 255, 0))
				.SetName ("HueFromColorDarkGreenHasGreenHue");
			yield return new TestCaseData (new CommonColor (0, 0, 127)).Returns (new CommonColor (0, 0, 255))
				.SetName ("HueFromColorDarkBlueHasBlueHue");
			yield return new TestCaseData (new CommonColor (127, 127, 0)).Returns (new CommonColor (255, 255, 0))
				.SetName ("HueFromColorDarkYellowHasYellowHue");
			yield return new TestCaseData (new CommonColor (127, 0, 127)).Returns (new CommonColor (255, 0, 255))
				.SetName ("HueFromColorDarkPurpleHasPurpleHue");
			yield return new TestCaseData (new CommonColor (0, 127, 127)).Returns (new CommonColor (0, 255, 255))
				.SetName ("HueFromColorDarkCyanHasCyanHue");
			// Saturated shades of non-primary colors map to themselves
			yield return new TestCaseData (new CommonColor (255, 142, 0)).Returns (new CommonColor (255, 142, 0))
				.SetName ("HueFromColorOrangeHueHasOrangeHue");
			yield return new TestCaseData (new CommonColor (255, 0, 142)).Returns (new CommonColor (255, 0, 142))
				.SetName ("HueFromColorPinkHueHasPinkHue");
			yield return new TestCaseData (new CommonColor (142, 255, 0)).Returns (new CommonColor (142, 255, 0))
				.SetName ("HueFromColorYellowishGreenHueHasYellowishGreenHue");
			yield return new TestCaseData (new CommonColor (0, 255, 142)).Returns (new CommonColor (0, 255, 142))
				.SetName ("HueFromColorBlueishGreenHueHasBlueishGreenHue");
			yield return new TestCaseData (new CommonColor (142, 0, 255)).Returns (new CommonColor (142, 0, 255))
				.SetName ("HueFromColorDeepPurpleHueHasDeepPurpleHue");
			yield return new TestCaseData (new CommonColor (0, 142, 255)).Returns (new CommonColor (0, 142, 255))
				.SetName ("HueFromColorBoringBlueHueHasBoringBlueHue");
			// More random colors (and also check alpha is 255)
			yield return new TestCaseData (new CommonColor (42, 142, 194, 78)).Returns (new CommonColor (0, 167, 255, 255))
				.SetName ("HueFromColorPetroleumBlueHasSaturatedBlueHue");
			yield return new TestCaseData (new CommonColor (142, 42, 194)).Returns (new CommonColor (167, 0, 255))
				.SetName ("HueFromColorUglyPurpleHasSaturatedUglyPurpleHue");
			yield return new TestCaseData (new CommonColor (142, 194, 42)).Returns (new CommonColor (167, 255, 0))
				.SetName ("HueFromColorKhakiHasSaturatedGreenHue");
			yield return new TestCaseData (new CommonColor (42, 194, 142)).Returns (new CommonColor (0, 255, 167))
				.SetName ("HueFromColorDullGreenHasSaturatedGreenHue");
			yield return new TestCaseData (new CommonColor (194, 42, 142)).Returns (new CommonColor (255, 0, 167))
				.SetName ("HueFromColorPurpleHasSaturatedPurpleHue");
			yield return new TestCaseData (new CommonColor (194, 142, 42)).Returns (new CommonColor (255, 167, 0))
				.SetName ("HueFromDullBrownHasOrangeHue");
		}

		[Test, TestCaseSource (typeof (CommonColorTests), "CMYKCases")]
		public CMYK CMYKTests (CommonColor color) => new CMYK(color.C, color.M, color.Y, color.K);

		private static IEnumerable CMYKCases() {
			yield return new TestCaseData (new CommonColor (0, 0, 0)).Returns (new CMYK(0, 0, 0, 1))
				.SetName ("CMYKBlack");
			yield return new TestCaseData (new CommonColor (255, 255, 255)).Returns (new CMYK (0, 0, 0, 0))
				.SetName ("CMYKWhite");
			yield return new TestCaseData (new CommonColor (255, 0, 0)).Returns (new CMYK (0, 1, 1, 0))
				.SetName ("CMYKRed");
			yield return new TestCaseData (new CommonColor (0, 255, 0)).Returns (new CMYK (1, 0, 1, 0))
				.SetName ("CMYKGreen");
			yield return new TestCaseData (new CommonColor (0, 0, 255)).Returns (new CMYK (1, 1, 0, 0))
				.SetName ("CMYKBlue");
			yield return new TestCaseData (new CommonColor (255, 255, 0)).Returns (new CMYK (0, 0, 1, 0))
				.SetName ("CMYKYellow");
			yield return new TestCaseData (new CommonColor (0, 255, 255)).Returns (new CMYK (1, 0, 0, 0))
				.SetName ("CMYKCyan");
			yield return new TestCaseData (new CommonColor (255, 0, 255)).Returns (new CMYK (0, 1, 0, 0))
				.SetName ("CMYKMagenta");
		}

		[Test, TestCaseSource (typeof (CommonColorTests), "ColorFromCMYKCases")]
		public CommonColor ColorFromCMYKTests (CMYK cmyk) => CommonColor.FromCMYK (cmyk.C, cmyk.M, cmyk.Y, cmyk.K);

		private static IEnumerable ColorFromCMYKCases ()
		{
			yield return new TestCaseData (new CMYK (0, 0, 0, 1)).Returns (new CommonColor (0, 0, 0))
				.SetName ("ColorFromCMYKBlack");
			yield return new TestCaseData (new CMYK (0, 0, 0, 0)).Returns (new CommonColor (255, 255, 255))
				.SetName ("ColorFromCMYKWhite");
			yield return new TestCaseData (new CMYK (0, 1, 1, 0)).Returns (new CommonColor (255, 0, 0))
				.SetName ("ColorFromCMYKRed");
			yield return new TestCaseData (new CMYK (1, 0, 1, 0)).Returns (new CommonColor (0, 255, 0))
				.SetName ("ColorFromCMYKGreen");
			yield return new TestCaseData (new CMYK (1, 1, 0, 0)).Returns (new CommonColor (0, 0, 255))
				.SetName ("ColorFromCMYKBlue");
			yield return new TestCaseData (new CMYK (0, 0, 1, 0)).Returns (new CommonColor (255, 255, 0))
				.SetName ("ColorFromCMYKYellow");
			yield return new TestCaseData (new CMYK (1, 0, 0, 0)).Returns (new CommonColor (0, 255, 255))
				.SetName ("ColorFromCMYKCyan");
			yield return new TestCaseData (new CMYK (0, 1, 0, 0)).Returns (new CommonColor (255, 0, 255))
				.SetName ("ColorFromCMYKMagenta");
		}

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

			public override string ToString () => $"{{ {C:F2}, {M:F2}, {Y:F2}, {K:F2} }}";
		}
	}
}
