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

		/// <summary>
		/// Gets a hue from this color.
		/// A hue has the highest component of the passed-in color at 255,
		/// the lowest at 0, and the intermediate one is interpolated.
		/// The result is a maximally saturated and bright color that looks
		/// like the original color.
		/// The precision of the mappin goes down as the color gets darker.
		/// All shades of grey get arbitrarily mapped to red.
		/// </summary>
		public CommonColor Hue
		{
			get {
				// Map grey to red
				if (R == G && G == B)
					return new CommonColor (255, 0, 0);

				var isRedMax = R >= G && R >= B;
				var isGreenMax = G >= R && G >= B;
				var isRedMin = R <= G && R <= B;
				var isGreenMin = G <= R && G <= B;
				if (isRedMax) {
					if (isGreenMin)
						return new CommonColor (255, 0, InterpolateComponent (B, G, R));
					else // blue is min
						return new CommonColor (255, InterpolateComponent (G, B, R), 0);
				}
				if (isGreenMax) {
					if (isRedMin)
						return new CommonColor (0, 255, InterpolateComponent (B, R, G));
					else // blue is min
						return new CommonColor (InterpolateComponent (R, B, G), 255, 0);
				}
				// blue is max
				if (isRedMin)
					return new CommonColor (0, InterpolateComponent (G, R, B), 255);
				else // green is min
					return new CommonColor (InterpolateComponent (R, G, B), 0, 255);
			}
		}

		/// <summary>
		/// A luminosity between 0 and 1, 1 being the measure for the hue of the color
		/// (the hue being a fully-saturated version of the same color), and 0 being
		/// the measure for black.
		/// </summary>
		public double Luminosity
		{
			get {
				var hue = Hue;
				var isRedMaxed = hue.R == 255;
				var isGreenMaxed = hue.G == 255;
				return isRedMaxed ? (double)R / 255 : isGreenMaxed ? (double)G / 255 : (double)B / 255;
			}
		}

		/// <summary>
		/// A saturation between 0 and 1, 1 being the measure for the hue of the color
		/// (the hue being a fully saturated version of the color), and 0 being the
		/// measure for white.
		/// </summary>
		public double Saturation
		{
			get {
				var hue = Hue;
				var luminosity = Luminosity;
				var isRedMin = hue.R == 0;
				var isGreenMin = hue.G == 0;

				return
					isRedMin ? (R / luminosity - 255) / (hue.R - 255)
					: isGreenMin ? (G / luminosity - 255) / (hue.G - 255)
					: (B / luminosity - 255) / (hue.B - 255);
			}
		}

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
			hue = hue.Hue; // Coerce the hue to be a real hue
			if (luminosity < 0 || luminosity > 1) {
				throw new ArgumentOutOfRangeException (nameof (luminosity));
			}
			if (saturation < 0 || saturation > 1) {
				throw new ArgumentOutOfRangeException (nameof (luminosity));
			}
			return new CommonColor (
						   (byte)((255 + (hue.R - 255) * saturation) * luminosity),
						   (byte)((255 + (hue.G - 255) * saturation) * luminosity),
						   (byte)((255 + (hue.B - 255) * saturation) * luminosity),
						   alpha
					   );
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
