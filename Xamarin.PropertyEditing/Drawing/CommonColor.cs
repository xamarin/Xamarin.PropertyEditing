using System;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace Xamarin.PropertyEditing.Drawing
{
	/// <summary>
	/// Describes a color.
	/// </summary>
	[Serializable]
	[TypeConverter(typeof(CommonColorToCommonBrushConverter))]
	public struct CommonColor : IEquatable<CommonColor>
	{
		public CommonColor (byte r, byte g, byte b, byte a = 255, string label = null)
			: this ((double)r, g, b, a, label) { }
		private CommonColor (double r, double g, double b, byte a = 255, string label = null)
		{
			this.r = r;
			this.g = g;
			this.b = b;
			A = a;
			Label = label;
			this.c = null;
			this.m = null;
			this.y = null;
			this.k = null;
			this.hueR = null;
			this.hueG = 0;
			this.hueB = 0;
			this.hue = null;
			this.lightness = null;
			this.brightness = null;
			this.saturation = null;
		}

		/// <summary>
		/// Red component
		/// </summary>
		public byte R => DoubleToByte (this.r);

		/// <summary>
		/// Green component
		/// </summary>
		public byte G => DoubleToByte (this.g);

		/// <summary>
		/// Blue component
		/// </summary>
		public byte B => DoubleToByte (this.b);

		/// <summary>
		/// Alpha channel
		/// </summary>
		public byte A { get; }

		/// <summary>
		/// An optional label for the color, that does not affect equality or anything else.
		/// </summary>
		public string Label { get; set; }

		/// <summary>
		/// Black component
		/// </summary>
		public double K => this.k ?? (this.k =
			(255 - (this.r >= this.g ? (this.r >= this.b ? this.r : this.b) : (this.g >= this.b ? this.g : this.b))) / 255).Value;
		/// <summary>
		/// Cyan component
		/// </summary>
		public double C => this.c ?? (this.c = Complement(this.r, K)).Value;

		/// <summary>
		/// Magenta component
		/// </summary>
		public double M => this.m ?? (this.m = Complement(this.g, K)).Value;

		/// <summary>
		/// Yellow component
		/// </summary>
		public double Y => this.y ?? (this.y = Complement(this.b, K)).Value;

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
				if (this.hue.HasValue)
					return CommonColor.FromHSB (this.hue.Value, 1, 1);
				
				if (this.hueR.HasValue)
					return new CommonColor(this.hueR.Value, this.hueG, this.hueB);

				// Map grey to red
				if (IsGrey) {
					this.hueR = 255;
					this.hueG = 0;
					this.hueB = 0;
					this.hue = 0;
					return new CommonColor(255, 0, 0);
				}

				var isRedMax = this.r >= this.g && this.r >= this.b;
				var isGreenMax = this.g >= this.r && this.g >= this.b;
				var isRedMin = this.r <= this.g && this.r <= this.b;
				var isGreenMin = this.g <= this.r && this.g <= this.b;

				CommonColor hueColor =
					isRedMax ?
						// Red is max
						isGreenMin ?
							// Green is min
							new CommonColor (255, 0, InterpolateComponent (this.b, this.g, this.r)) :
							// Blue is min
							new CommonColor (255, InterpolateComponent (this.g, this.b, this.r), 0) :
					isGreenMax ?
						// Green is max
						isRedMin ?
							// Red is min
							new CommonColor (0, 255, InterpolateComponent (this.b, this.r, this.g)) :
							// Blue is min
							new CommonColor (InterpolateComponent (this.r, this.b, this.g), 255, 0) :
					// Blue is max
					isRedMin ?
						// Red is min
						new CommonColor (0, InterpolateComponent (this.g, this.r, this.b), 255) :
						// Green is min
						new CommonColor (InterpolateComponent (this.r, this.g, this.b), 0, 255);

				this.hueR = hueColor.r;
				this.hueG = hueColor.g;
				this.hueB = hueColor.b;
				return hueColor;
			}
		}

		/// <summary>
		/// The hue for this color, as an angle between 0 and 360 degrees.
		/// </summary>
		public double Hue {
			get {
				if (this.hue.HasValue) return this.hue.Value;

				// Map grey to 0 degrees
				if (IsGrey) {
					this.hueR = 255;
					this.hueG = 0;
					this.hueB = 0;
					this.hue = 0;
					return 0;
				}

				var isRedMax = this.r >= this.g && this.r >= this.b;
				var isGreenMax = this.g >= this.r && this.g >= this.b;
				var isRedMin = this.r <= this.g && this.r <= this.b;
				var isGreenMin = this.g <= this.r && this.g <= this.b;

				var d = (isRedMax ? this.r : isGreenMax ? this.g : this.b) - (isRedMin ? this.r : isGreenMin ? this.g : this.b);

				this.hue =
					isRedMax ? (Mod((this.g - this.b) / d, 6)) * 60 :
					isGreenMax ? (((this.b - this.r)/ d) + 2) * 60 :
					(((this.r - this.g) / d) + 4) * 60;

				return this.hue.Value;
			}
		}

		/// <summary>
		/// The lightness of the color, where white is 1, black is 0, and primary colors are 0.5.
		/// </summary>
		public double Lightness {
			get {
				if (this.lightness.HasValue) return this.lightness.Value;

				var isRedMax = this.r >= this.g && this.r >= this.b;
				var isGreenMax = this.g >= this.r && this.g >= this.b;
				var isRedMin = this.r <= this.g && this.r <= this.b;
				var isGreenMin = this.g <= this.r && this.g <= this.b;

				this.lightness =
					((isRedMax ? this.r : isGreenMax ? this.g : this.b) + (isRedMin ? this.r : isGreenMin ? this.g : this.b)) / 510;

				return this.lightness.Value;
			}
		}

		/// <summary>
		/// A brightness between 0 and 1, 1 being the measure for the hue of the color
		/// (the hue being a fully-saturated version of the same color), and 0 being
		/// the measure for black.
		/// </summary>
		public double Brightness {
			get {
				if (this.brightness.HasValue) return this.brightness.Value;

				CommonColor hue = HueColor;
				var isRedMaxed = hue.R == 255;
				var isGreenMaxed = hue.G == 255;
				return (this.brightness =
					isRedMaxed ? this.r / 255 :
					isGreenMaxed ? this.g / 255 :
					this.b / 255
					).Value;
			}
		}

		/// <summary>
		/// A saturation between 0 and 1, 1 being the measure for the hue of the color
		/// (the hue being a fully saturated version of the color), and 0 being the
		/// measure for white.
		/// </summary>
		public double Saturation {
			get {
				if (this.saturation.HasValue) return this.saturation.Value;

				if (IsGrey) return (this.saturation = 0).Value;

				var isRedMax = this.r >= this.g && this.r >= this.b;
				var isGreenMax = this.g >= this.r && this.g >= this.b;
				var isRedMin = this.r <= this.g && this.r <= this.b;
				var isGreenMin = this.g <= this.r && this.g <= this.b;

				var max = isRedMax ? this.r : isGreenMax ? this.g : this.b;
				var d = (max - (isRedMin ? this.r : isGreenMin ? this.g : this.b));

				this.saturation = d / max;

				return this.saturation.Value;
			}
		}

		private readonly double r;
		private readonly double g;
		private readonly double b;
		private double? k;
		private double? c;
		private double? m;
		private double? y;
		private double? hue;
		private double? hueR;
		private double hueG, hueB;
		private double? lightness;
		private double? brightness;
		private double? saturation;

		private double Complement (double component, double k)
		{
			if (k == 1) return 0;
			var val = (1 - k - component / 255) / (1 - k);
			return val < 0 ? 0 : val > 1 ? 1 : val;
		}

		private static readonly int[][] redRanges = new[] { new[] { 0, 60 }, new[] { 300, 360 } };
		private static readonly int[][] greenRanges = new[] { new[] { 60, 180 } };
		private static readonly int[][] blueRanges = new[] { new[] { 180, 300 } };

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
		private static double GetHueComponent (double hue, int[][] intervals)
		{
			if (hue < 0 || hue > 360)
				throw new ArgumentOutOfRangeException (nameof (hue), "Position must be between 0 and 6.");
			foreach (var interval in intervals) {
				// Component is 255 inside the interval
				if (hue >= interval[0] && hue <= interval[1])
					return 255;
				// Component linearly grows from 0 to 255 60 degrees left of the interval
				if (hue >= interval[0] - 60 && hue < interval[0])
					return (hue - interval[0] + 60) * 255 / 60;
				// Component linearly falls from 255 to 0 60 degrees right of the interval
				if (hue > interval[1] && hue <= interval[1] + 60)
					return 255 - (hue - interval[1]) * 255 / 60;
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
			if (hueColor.hue.HasValue) return hueColor.hue.Value;
			
			if (hueColor.B == 0) {
				// We're between 0 and 120
				if (hueColor.R == 255) {
					// We're between 0 and 60, and green is on the rising phase
					return hueColor.g * 60 / 255;
				}
				// We're between 60 and 120, and red is on the declining phase
				return ((255 - hueColor.r) * 60 / 255 + 60);
			}
			else if (hueColor.R == 0) {
				// We're between 120 and 240
				if (hueColor.G == 255) {
					// We're between 120 and 180, and blue is on the rise
					return (hueColor.b * 60 / 255 + 120);
				}
				// We're between 180 and 240, and green is declining
				return ((255 - hueColor.g) * 60 / 255 + 180);
			}
			// We're between 240 and 360
			if (hueColor.B == 255) {
				// We're between 240 and 300, and red is on the rise
				return (hueColor.r * 60 / 255 + 240);
			}
			// We're between 300 and 360, and blue is declining
			return ((255 - hueColor.b) * 60 / 255 + 300);
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
		static double InterpolateComponent (double component, double lowest, double highest)
		{
			var delta = highest - lowest;
			return delta == 0 ? highest : (component - lowest) * 255 / delta;
		}

		public string ToRgbaHex () => $"#{R:X2}{G:X2}{B:X2}{A:X2}";

		public string ToArgbHex () => (A == 255) ? $"#{R:X2}{G:X2}{B:X2}" : $"#{A:X2}{R:X2}{G:X2}{B:X2}";

		public static bool TryParseArgbHex (string value, out CommonColor color)
		{
			if (Regex.IsMatch (value, @"^#(([A-Fa-f0-9]{2}){3}|[A-Fa-f0-9]{3}|[A-Fa-f0-9]{4}|([A-Fa-f0-9]{2}){4})$")) {
				var hex = value.Substring (1);
				switch (hex.Length) {
				case 3:
					hex = $"FF{hex[0]}{hex[0]}{hex[1]}{hex[1]}{hex[2]}{hex[2]}";
					goto case 8;
				case 6:
					hex = "FF" + hex;
					goto case 8;
				case 4:
					hex = $"{hex[0]}{hex[0]}{hex[1]}{hex[1]}{hex[2]}{hex[2]}{hex[3]}{hex[3]}";
					goto case 8;
				case 8:
					var v = Convert.ToInt32 (hex, 16);
					color = new CommonColor (
						a: (byte)(v >> 24),
						r: (byte)(v >> 16),
						g: (byte)(v >> 8),
						b: (byte)v);

					return true;
				}
			}
			color = CommonColor.Black;
			return false;
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
					   255 * (1 - c) * (1 - k),
					   255 * (1 - m) * (1 - k),
					   255 * (1 - y) * (1 - k),
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

			var color = new CommonColor (255 * (r + m), 255 * (g + m), 255 * (b + m), alpha) {
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
				throw new ArgumentOutOfRangeException (nameof (brightness));
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

			var color = new CommonColor (255 * (r + m), 255 * (g + m), 255 * (b + m), alpha) {
				// Pre-cache HLS components, since we know them, and it will improve round-tripping
				hue = hue,
				brightness = brightness,
				saturation = saturation
			};
			return color;
		}

		public static CommonColor Black = new CommonColor (0, 0, 0);
		public static CommonColor White = new CommonColor (255, 255, 255);

		private static double Mod (double a, double b)	=> a - b * Math.Floor (a / b);

		private static byte DoubleToByte(double? d)
		{
			if (!d.HasValue) return 0;
			var round = Math.Round (d.Value);
			return (byte)(round < 0 ? 0 : round > 255 ? 255 : round);
		}

		public override bool Equals (object obj)
			=> obj == null ? false : !(obj is CommonColor otherColor) ? false : Equals (otherColor, false);

		public bool Equals (CommonColor other) => Equals (other, false);

		public bool Equals (CommonColor other, bool ignoreAlpha)
		{
			return (ignoreAlpha || A == other.A) &&
				   R == other.R &&
				   G == other.G &&
				   B == other.B;
		}

		public static bool operator == (CommonColor left, CommonColor right) => Equals (left, right);
		public static bool operator != (CommonColor left, CommonColor right) => !Equals (left, right);

		public static double SquaredDistance (CommonColor left, CommonColor right)
			=> (left.r - right.r) * (left.r - right.r)
			+ (left.g - right.g) * (left.g - right.g)
			+ (left.b - right.b) * (left.b - right.b);



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

		public override string ToString () => ToArgbHex ();
	}
}
