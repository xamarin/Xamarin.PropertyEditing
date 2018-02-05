using System;

namespace Xamarin.PropertyEditing.Drawing
{
	/// <summary>
	/// Describes a color.
	/// </summary>
	[Serializable]
	public struct CommonColor : IEquatable<CommonColor>
	{
		public CommonColor (byte r, byte g, byte b, byte a = 255)
		{
			A = a;
			R = r;
			G = g;
			B = b;
			c = null;
			m = null;
			y = null;
			k = null;
			hueR = null;
			hueG = 0;
			hueB = 0;
			hue = null;
			lightness = null;
			brightness = null;
			saturation = null;
		}

		/// <summary>
		/// Alpha channel
		/// </summary>
		public byte A { get; }
		/// <summary>
		/// Red component
		/// </summary>
		public byte R { get; }
		/// <summary>
		/// Green component
		/// </summary>
		public byte G { get; }
		/// <summary>
		/// Blue component
		/// </summary>
		public byte B { get; }

		double? k;
		/// <summary>
		/// Black component
		/// </summary>
		public double K => k ?? (k = (double)(255 - (R >= G ? (R >= B ? R : B) : (G >= B ? G : B))) / 255).Value;

		double Complement(byte component, double k)
		{
			if (k == 1) return 0;
			var val = (1 - k - (double)component / 255) / (1 - k);
			if (val < 0) return 0;
			if (val > 1) return 1;
			return val;
		}

		double? c;
		/// <summary>
		/// Cyan component
		/// </summary>
		public double C => c ?? (c = Complement(R, K)).Value;

		double? m;
		/// <summary>
		/// Magenta component
		/// </summary>
		public double M => m ?? (m = Complement(G, K)).Value;

		double? y;
		/// <summary>
		/// Yellow component
		/// </summary>
		public double Y => y ?? (y = Complement(B, K)).Value;

		static readonly CommonColor Red = new CommonColor (255, 0, 0);

		byte? hueR;
		byte hueG, hueB;
		/// <summary>
		/// Gets a hue from this color.
		/// A hue has the highest component of the passed-in color at 255,
		/// the lowest at 0, and the intermediate one is interpolated.
		/// The result is a maximally saturated and bright color that looks
		/// like the original color.
		/// The precision of the mappin goes down as the color gets darker.
		/// All shades of grey get arbitrarily mapped to red.
		/// </summary>
		public CommonColor HueColor {
			get {
				if (hueR.HasValue) return new CommonColor(hueR.Value, hueG, hueB);

				// Map grey to red
				if (IsGrey) {
					hueR = 255;
					hueG = 0;
					hueB = 0;
					hue = 0;
					return Red;
				}

				var isRedMax = R >= G && R >= B;
				var isGreenMax = G >= R && G >= B;
				var isRedMin = R <= G && R <= B;
				var isGreenMin = G <= R && G <= B;

				CommonColor hueColor =
					isRedMax ?
						// Red is max
						isGreenMin ?
							// Green is min
							new CommonColor (255, 0, InterpolateComponent (B, G, R)) :
							// Blue is min
							new CommonColor (255, InterpolateComponent (G, B, R), 0) :
					isGreenMax ?
						// Green is max
						isRedMin ?
							// Red is min
							new CommonColor (0, 255, InterpolateComponent (B, R, G)) :
							// Blue is min
							new CommonColor (InterpolateComponent (R, B, G), 255, 0) :
					// Blue is max
					isRedMin ?
						// Red is min
						new CommonColor (0, InterpolateComponent (G, R, B), 255) :
						// Green is min
						new CommonColor (InterpolateComponent (R, G, B), 0, 255);

				hueR = hueColor.R;
				hueG = hueColor.G;
				hueB = hueColor.B;
				return hueColor;
			}
		}

		double? hue;
		/// <summary>
		/// The hue for this color, as an angle between 0 and 360 degrees.
		/// </summary>
		public double Hue {
			get {
				if (hue.HasValue) return hue.Value;

				// Map grey to 0 degrees
				if (IsGrey) {
					hueR = 255;
					hueG = 0;
					hueB = 0;
					hue = 0;
					return 0;
				}

				var isRedMax = R >= G && R >= B;
				var isGreenMax = G >= R && G >= B;
				var isRedMin = R <= G && R <= B;
				var isGreenMin = G <= R && G < B;
				var d = ((double)(isRedMax ? R : isGreenMax ? G : B) - (isRedMin ? R : isGreenMin ? G : B));

				hue =
					isRedMax ? (Mod((G - B) / d, 6)) * 60 :
					isGreenMax ? (((B - R)/ d) + 2) * 60 :
					(((R - G) / d) + 4) * 60;

				return hue.Value;
			}
		}

		double? lightness;
		/// <summary>
		/// The lightness of the color, where white is 1, black is 0, and primary colors are 0.5.
		/// </summary>
		public double Lightness {
			get {
				if (lightness.HasValue) return lightness.Value;

				var isRedMax = R >= G && R >= B;
				var isGreenMax = G >= R && G >= B;
				var isRedMin = R <= G && R <= B;
				var isGreenMin = G <= R && G <= B;

				lightness = ((double)(isRedMax ? R : isGreenMax ? G : B) + (isRedMin ? R : isGreenMin ? G : B)) / 510;

				return lightness.Value;
			}
		}

		double? brightness;
		/// <summary>
		/// A brightness between 0 and 1, 1 being the measure for the hue of the color
		/// (the hue being a fully-saturated version of the same color), and 0 being
		/// the measure for black.
		/// </summary>
		public double Brightness {
			get {
				if (brightness.HasValue) return brightness.Value;

				CommonColor hue = HueColor;
				var isRedMaxed = hue.R == 255;
				var isGreenMaxed = hue.G == 255;
				return (brightness =
					isRedMaxed ? (double)R / 255 :
					isGreenMaxed ? (double)G / 255 :
					(double)B / 255
					).Value;
			}
		}

		double? saturation;
		/// <summary>
		/// A saturation between 0 and 1, 1 being the measure for the hue of the color
		/// (the hue being a fully saturated version of the color), and 0 being the
		/// measure for white.
		/// </summary>
		public double Saturation {
			get {
				if (saturation.HasValue) return saturation.Value;

				if (IsGrey) return (saturation = 0).Value;

				var isRedMax = R >= G && R >= B;
				var isGreenMax = G >= R && G >= B;
				var isRedMin = R <= G && R <= B;
				var isGreenMin = G <= R && G <= B;

				var max = (double)(isRedMax ? R : isGreenMax ? G : B);
				var d = (max - (isRedMin ? R : isGreenMin ? G : B));

				saturation = d / max;

				return saturation.Value;
			}
		}

		static readonly int[][] redRanges = new[] { new[] { 0, 60 }, new[] { 300, 360 } };
		static readonly int[][] greenRanges = new[] { new[] { 60, 180 } };
		static readonly int[][] blueRanges = new[] { new[] { 180, 300 } };

		/// <summary>
		/// Finds the hue color from a hue angle between 0 and 360 degrees.
		/// 
		/// The hue dial is a gradient going through red, yellow, lime, cyan, blue, magenta, red.
		/// This means the following variations for red, green, and blue components:
		/// Hue: 0 |  60 | 120 | 180 | 240 | 300 | 360
		/// -------|-----|-----|-----|-----|-----|-----
		/// R: 255 | 255 |   0 |   0 |   0 | 255 | 255
		/// G:   0 | 255 | 255 | 255 |   0 |   0 |   0
		/// B:   0 |   0 |   0 | 255 | 255 | 255 |   0
		/// </summary>
		/// <param name="hue">The horizontal position on the hue picker, between 0 and 1</param>
		/// <returns>The hue</returns>
		public static CommonColor GetHueColorFromHue (double hue)
		{
			hue = Math.Min (Math.Max (0, hue), 360);
			return new CommonColor (
				GetHueComponent (hue, redRanges),
				GetHueComponent (hue, greenRanges),
				GetHueComponent (hue, blueRanges)
			);
		}

		/// <summary>
		/// Gets a color component between 0 and 255 based on a position
		/// between 0 and 360, and a set of ranges where the component is maxed out.
		/// The component varies from 0 to 255 over the position range 1 unit to the
		/// left of each interval, and from 255 to 0 over the position range 1 unit to
		/// the right of each interval
		/// </summary>
		/// <param name="hue">The hue, between 0 and 360</param>
		/// <param name="intervals">A set of intervals where the component is 255.</param>
		/// <returns>The value of the component.</returns>
		static byte GetHueComponent (double hue, int[][] intervals)
		{
			if (hue < 0 || hue > 360)
				throw new ArgumentOutOfRangeException (nameof (hue), "Position must be between 0 and 6.");
			foreach (int[] interval in intervals) {
				// Component is 255 inside the interval
				if (hue >= interval[0] && hue <= interval[1])
					return 255;
				// Component linearly grows from 0 to 255 60 degrees left of the interval
				if (hue >= interval[0] - 60 && hue < interval[0])
					return (byte)((hue - interval[0] + 60) * 255 / 60);
				// Component linearly falls from 255 to 0 60 degrees right of the interval
				if (hue > interval[1] && hue <= interval[1] + 60)
					return (byte)(255 - (hue - interval[1]) * 255 / 60);
			}
			// Otherwise, it's zero
			return 0;
		}

		/// <summary>
		/// Finds a hue between 0 and 360 degrees from a hue color.
		/// </summary>
		/// <param name="hueColor">The hue color</param>
		/// <returns>The hue between 0 and 360 degrees</returns>
		public static double GetHueFromHueColor (CommonColor hueColor)
		{
			if (hueColor.B == 0) {
				// We're between 0 and 120
				if (hueColor.R == 255) {
					// We're between 0 and 60, and green is on the rising phase
					return (double)hueColor.G * 60 / 255;
				}
				// We're between 60 and 120, and red is on the declining phase
				return ((double)(255 - hueColor.R) * 60 / 255 + 60);
			}
			else if (hueColor.R == 0) {
				// We're between 120 and 240
				if (hueColor.G == 255) {
					// We're between 120 and 180, and blue is on the rise
					return ((double)hueColor.B * 60 / 255 + 120);
				}
				// We're between 180 and 240, and green is declining
				return ((double)(255 - hueColor.G) * 60 / 255 + 180);
			}
			// We're between 240 and 360
			if (hueColor.B == 255) {
				// We're between 240 and 300, and red is on the rise
				return ((double)hueColor.R * 60 / 255 + 240);
			}
			// We're between 300 and 360, and blue is declining
			return ((double)(255 - hueColor.B) * 60 / 255 + 300);
		}

		/// <summary>
		/// True if the color is a shade of grey.
		/// </summary>
		public bool IsGrey => R == G && G == B;

		/// <summary>
		/// Computes where the third component should be if the top is mapped
		/// to 255 and the lowest gets mapped to 0.
		/// </summary>
		/// <param name="component">The third component's value</param>
		/// <param name="lowest">The lowest component value</param>
		/// <param name="highest">The highest component value</param>
		/// <returns>The interpolated third component</returns>
		static byte InterpolateComponent (byte component, byte lowest, byte highest)
		{
			var delta = highest - lowest;
			if (delta == 0) return highest;
			return (byte)((component - lowest) * 255 / delta);
		}

		/// <summary>
		/// Creates a color from cyan, magenta, yellow, and black components
		/// </summary>
		/// <param name="c">The cyan component, between 0 and 1</param>
		/// <param name="m">The magenta component, between 0 and 1</param>
		/// <param name="y">The yellow component, between 0 and 1</param>
		/// <param name="k">The black component, between 0 and 1</param>
		/// <param name="alpha">The alpha channel value</param>
		/// <returns>The color</returns>
		public static CommonColor FromCMYK (double c, double m, double y, double k, byte alpha = 255)
		{
			if (c < 0 || c > 1) {
				throw new ArgumentOutOfRangeException (nameof (c));
			}
			if (m < 0 || m > 1) {
				throw new ArgumentOutOfRangeException (nameof (m));
			}
			if (y < 0 || y > 1) {
				throw new ArgumentOutOfRangeException (nameof (y));
			}
			if (k < 0 || k > 1) {
				throw new ArgumentOutOfRangeException (nameof (k));
			}
			var color = new CommonColor (
					   (byte)(255 * (1 - c) * (1 - k)),
					   (byte)(255 * (1 - m) * (1 - k)),
					   (byte)(255 * (1 - y) * (1 - k)),
					   alpha) {
				// pre-cache CMYK components, since we know them, and it will improve round-tripping
				c = c,
				m = m,
				y = y,
				k = k
			};
			return color;
		}

		/// <summary>
		/// Creates a color from hue, lightness, and saturation.
		/// </summary>
		/// <param name="hue">The hue between 0 and 360</param>
		/// <param name="lightness">The lightness between 0 and 1</param>
		/// <param name="saturation">The saturation between 0 and 1</param>
		/// <param name="alpha">The alpha channel value</param>
		/// <returns>The color</returns>
		public static CommonColor FromHLS (double hue, double lightness, double saturation, byte alpha = 255)
		{
			if (hue < 0 || hue > 360) {
				throw new ArgumentOutOfRangeException (nameof (hue));
			}
			if (lightness < 0 || lightness > 1) {
				throw new ArgumentOutOfRangeException (nameof (lightness));
			}
			if (saturation < 0 || saturation > 1) {
				throw new ArgumentOutOfRangeException (nameof (saturation));
			}

			var c = (1 - Math.Abs (2 * lightness - 1)) * saturation;
			var x = c * (1 - Math.Abs (Mod(hue / 60, 2) - 1));
			var m = lightness - c / 2;

			var r =
				hue < 60 || hue >= 300 ? c :
				hue < 120 || hue >= 240 ? x :
				0;
			var g =
				hue >= 240 ? 0 :
				hue < 60 || hue >= 180 ? x :
				c;
			var b =
				hue < 120 ? 0 :
				hue < 180 || hue >= 300 ? x :
				c;

			var color = new CommonColor ((byte)(255 * (r + m)), (byte)(255 * (g + m)), (byte)(255 * (b + m)), alpha) {
				// Pre-cache HLS components, since we know them, and it will improve round-tripping
				hue = hue,
				lightness = lightness,
				saturation = saturation
			};
			return color;
		}

		/// <summary>
		/// Creates a color from hue, saturation, and brightness.
		/// </summary>
		/// <param name="hue">The hue between 0 and 360</param>
		/// <param name="saturation">The saturation between 0 and 1</param>
		/// <param name="brightness">The brightness between 0 and 1</param>
		/// <param name="alpha">The alpha channel value</param>
		/// <returns>The color</returns>
		public static CommonColor FromHSB (double hue, double saturation, double brightness, byte alpha = 255)
		{
			if (hue < 0 || hue > 360) {
				throw new ArgumentOutOfRangeException (nameof (hue));
			}
			if (saturation < 0 || saturation > 1) {
				throw new ArgumentOutOfRangeException (nameof (saturation));
			}
			if (brightness < 0 || brightness > 1) {
				throw new ArgumentOutOfRangeException (nameof (lightness));
			}

			var c = brightness * saturation;
			var x = c * (1 - Math.Abs (Mod(hue / 60, 2) - 1));
			var m = brightness - c;

			var r =
				hue < 60 || hue >= 300 ? c :
				hue < 120 || hue >= 240 ? x :
				0;
			var g =
				hue >= 240 ? 0 :
				hue < 60 || hue >= 180 ? x :
				c;
			var b =
				hue < 120 ? 0 :
				hue < 180 || hue >= 300 ? x :
				c;

			var color = new CommonColor ((byte)(255 * (r + m)), (byte)(255 * (g + m)), (byte)(255 * (b + m)), alpha) {
				// Pre-cache HLS components, since we know them, and it will improve round-tripping
				hue = hue,
				brightness = brightness,
				saturation = saturation
			};
			return color;
		}

		static double Mod (double a, double b)	=> a - b * Math.Floor (a / b);

		public override bool Equals (object obj)
		{
			if (obj == null) return false;
			if (!(obj is CommonColor)) return false;
			return base.Equals ((CommonColor)obj);
		}

		public bool Equals (CommonColor other)
		{
			return A == other.A &&
				   R == other.R &&
				   G == other.G &&
				   B == other.B;
		}

		public static bool operator == (CommonColor left, CommonColor right) => Equals (left, right);
		public static bool operator != (CommonColor left, CommonColor right) => !Equals (left, right);

		public override int GetHashCode ()
		{
			var hashCode = 466501756;
			unchecked {
				hashCode = hashCode * -1521134295 + A.GetHashCode ();
				hashCode = hashCode * -1521134295 + R.GetHashCode ();
				hashCode = hashCode * -1521134295 + G.GetHashCode ();
				hashCode = hashCode * -1521134295 + B.GetHashCode ();
			}
			return hashCode;
		}

		public override string ToString ()
		{
			return $"{{R: {R}, G: {G}, B: {B}, A: {A}}}";
		}
	}
}
