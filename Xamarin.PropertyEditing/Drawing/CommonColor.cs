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
			luminosity = null;
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

		double? luminosity;
		/// <summary>
		/// A luminosity between 0 and 1, 1 being the measure for the hue of the color
		/// (the hue being a fully-saturated version of the same color), and 0 being
		/// the measure for black.
		/// </summary>
		public double Luminosity {
			get {
				if (luminosity.HasValue) return luminosity.Value;
				CommonColor hue = HueColor;
				var isRedMaxed = hue.R == 255;
				var isGreenMaxed = hue.G == 255;
				return (luminosity =
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
				CommonColor hue = HueColor;
				var luminosity = Luminosity;
				var isRedMin = hue.R == 0;
				var isGreenMin = hue.G == 0;

				return (saturation =
					isRedMin ? (R / luminosity - 255) / (hue.R - 255)
					: isGreenMin ? (G / luminosity - 255) / (hue.G - 255)
					: (B / luminosity - 255) / (hue.B - 255)
					).Value;
			}
		}

		/// <summary>
		/// True if the color is a shade of grey.
		/// </summary>
		public bool IsGrey => R == G && G == B;

		/// <summary>
		/// Creates a color from hue, saturation, and luminosity.
		/// </summary>
		/// <param name="hue">The hue</param>
		/// <param name="luminosity">The luminosity between 0 and 1</param>
		/// <param name="saturation">The saturation between 0 and 1</param>
		/// <param name="alpha">The alpha channel value</param>
		/// <returns>The color</returns>
		public static CommonColor From (CommonColor hue, double luminosity, double saturation, byte alpha = 255)
		{
			hue = hue.HueColor; // Coerce the hue to be a real hue
			if (luminosity < 0 || luminosity > 1) {
				throw new ArgumentOutOfRangeException (nameof (luminosity));
			}
			if (saturation < 0 || saturation > 1) {
				throw new ArgumentOutOfRangeException (nameof (saturation));
			}
			var color = new CommonColor (
						   (byte)((255 + (hue.R - 255) * saturation) * luminosity),
						   (byte)((255 + (hue.G - 255) * saturation) * luminosity),
						   (byte)((255 + (hue.B - 255) * saturation) * luminosity),
						   alpha
					   ) {
				// Pre-cache HLS components, since we know them, and it will improve round-tripping
				hueR = hue.R,
				hueG = hue.G,
				hueB = hue.B,
				luminosity = luminosity,
				saturation = saturation
			};
			return color;
		}

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

		public override bool Equals (object obj)
		{
			if (obj == null) return false;
			if (!(obj is CommonThickness)) return false;
			return base.Equals ((CommonThickness)obj);
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
