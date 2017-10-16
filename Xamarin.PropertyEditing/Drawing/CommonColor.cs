using System;

namespace Xamarin.PropertyEditing.Drawing
{
	/// <summary>
	/// Describes a color.
	/// </summary>
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
		public byte A { get; set; }
		/// <summary>
		/// Red component
		/// </summary>
		public byte R { get; set; }
		/// <summary>
		/// Green component
		/// </summary>
		public byte G { get; set; }
		/// <summary>
		/// Blue component
		/// </summary>
		public byte B { get; set; }

		/// <summary>
		/// Gets a hue from this color.
		/// A hue has the highest component of the passed-in color at 255,
		/// the lowest at 0, and the intermediate one is interpolated.
		/// The result is a maximally saturated and bright color that looks
		/// like the original color.
		/// The precision of the mappin goes down as the color gets darker.
		/// All shades of grey get arbitrarily mapped to red.
		/// </summary>
		/// <returns>The hue</returns>
		public CommonColor ToHue ()
		{
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
