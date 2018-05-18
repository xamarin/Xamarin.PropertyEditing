using System;
using System.Collections.Generic;
using System.Linq;
using CoreAnimation;
using CoreGraphics;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.Mac
{
	public abstract class ChannelEditor
	{
		public string Name { get; }
		public double MinimumValue { get; }
		public double MaximumValue { get; }
		public double IncrementValue { get; }

		static IEnumerable<double> LerpSteps (double min, double max, int steps)
			=> Enumerable.Range (0, steps).Select (v => {
				var pos = v / (double)steps;
				return max * pos - min * (1 - pos);
			});

		public ChannelEditor (string name, double min, double max, double increment)
		{
			MinimumValue = min;
			MaximumValue = max;
			IncrementValue = increment;
			Name = name;
		}

		public void UpdateGradientLayer (CAGradientLayer layer, CommonColor color)
		{
			var c = color.UpdateRGB (a: 255);

			layer.Colors = LerpSteps (MinimumValue, MaximumValue, 7)
				.Select (value => UpdateColorFromValue (c, value).ToCGColor ()).ToArray ();
		}

		public double InvLerp (CGPoint start, CGPoint end, CGPoint loc)
		{
			var a = new CGVector (end.X - start.X, end.Y - start.Y);
			var b = new CGVector (loc.X - start.X, loc.Y - start.Y);
			var dot = a.dx * b.dx + a.dy * b.dy;
			var len = Math.Sqrt (a.dx * a.dx + a.dy * a.dy);
			return dot / len;
		}

		public static double InvLerp (double start, double end, double value)
		=> (value - start) / (end - start);

		public static double Lerp (double start, double end, double amount)
		=> end * amount - start * (1 - amount);

		public static CGPoint Lerp (CGPoint start, CGPoint end, double amount)
		=> new CGPoint (
			start.X + (end.X - start.X) * amount,
			start.Y + (end.Y - start.Y) * amount);

		public double ValueFromLocation (CAGradientLayer layer, CGPoint loc)
		{
			var rect = layer.Frame;
			var unitLoc = new CGPoint (
				(loc.X - rect.X) / rect.Width,
				(loc.Y - rect.Y) / rect.Height);

			return Clamp (Lerp (MinimumValue, MaximumValue, InvLerp (layer.StartPoint, layer.EndPoint, unitLoc)));
		}

		public CommonColor UpdateColorFromLocation (CAGradientLayer layer, CommonColor color, CGPoint loc)
		=> UpdateColorFromValue (color, ValueFromLocation (layer, loc));

		public CGPoint LocationFromColor (CAGradientLayer layer, CommonColor color)
		{
			var pos = ValueFromColor (color);

			var amount = InvLerp (MinimumValue, MaximumValue, pos);
			var unitLoc = Lerp (layer.StartPoint, layer.EndPoint, amount);

			return new CGPoint (
				layer.Frame.X + unitLoc.X * layer.Frame.Width,
				layer.Frame.Y + unitLoc.Y * layer.Frame.Height);
		}

		public double Clamp (double value)
		=> Math.Max (MinimumValue, Math.Min (MaximumValue, value));

		public abstract CommonColor UpdateColorFromValue (CommonColor color, double value);
		public abstract double ValueFromColor (CommonColor color);
	}

	class RedChannelEditor : ChannelEditor
	{
		public RedChannelEditor () : base ("R", 0d, 255d, 1d)
		{
		}

		public override double ValueFromColor (CommonColor color)
		=> (double)color.R;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateRGB (r: (byte)Clamp (value));
	}

	class GreenChannelEditor : ChannelEditor
	{
		public GreenChannelEditor () : base ("G", 0d, 255d, 1d)
		{
		}

		public override double ValueFromColor (CommonColor color)
		=> (double)color.G;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateRGB (g: (byte)Clamp (value));
	}

	class BlueChannelEditor : ChannelEditor
	{
		public BlueChannelEditor () : base ("B", 0d, 255d, 1d)
		{
		}

		public override double ValueFromColor (CommonColor color)
		=> (double)color.B;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateRGB (b: (byte)Clamp (value));
	}

	class AlphaChannelEditor : ChannelEditor
	{
		public AlphaChannelEditor () : base ("A", 0d, 255d, 1d)
		{
		}

		public override double ValueFromColor (CommonColor color)
		=> (double)color.A;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateRGB (a: (byte)Clamp (value));
	}

	class CyanChannelEditor : ChannelEditor
	{
		public CyanChannelEditor () : base ("C", 0d, 1d, .001d)
		{
		}

		public override double ValueFromColor (CommonColor color)
		=> color.C;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateCMYK (c: Clamp (value));
	}

	class MagentaChannelEditor : ChannelEditor
	{
		public MagentaChannelEditor () : base ("M", 0d, 1d, .001d)
		{
		}

		public override double ValueFromColor (CommonColor color)
		=> color.M;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateCMYK (m: Clamp (value));
	}

	class YellowChannelEditor : ChannelEditor
	{
		public YellowChannelEditor () : base ("Y", 0d, 1d, .001d)
		{
		}

		public override double ValueFromColor (CommonColor color)
		=> color.Y;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateCMYK (y: Clamp (value));
	}

	class BlackChannelEditor : ChannelEditor
	{
		public BlackChannelEditor () : base ("K", 0d, 1d, .001d)
		{
		}

		public override double ValueFromColor (CommonColor color)
		=> color.K;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateCMYK (k: Clamp (value));
	}

	class HsbHueChannelEditor : ChannelEditor
	{
		public HsbHueChannelEditor () : base ("H", 0d, 360d, 1d)
		{
		}

		public override double ValueFromColor (CommonColor color)
		=> color.Hue;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateHSB (hue: Clamp (value));
	}

	class HsbSaturationChannelEditor : ChannelEditor
	{
		public HsbSaturationChannelEditor () : base ("S", 0d, 1d, .001d)
		{
		}

		public override double ValueFromColor (CommonColor color)
		=> color.Saturation;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateHSB (saturation: Clamp (value));
	}

	class HsbBrightnessChannelEditor : ChannelEditor
	{
		public HsbBrightnessChannelEditor () : base ("B", 0d, 1d, .001d)
		{
		}

		public override double ValueFromColor (CommonColor color)
		=> color.Brightness;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateHSB (brightness: Clamp (value));
	}

	class HsbAlphaChannelEditor : ChannelEditor
	{
		public HsbAlphaChannelEditor () : base ("A", 0d, 255d, 1d)
		{
		}

		public override double ValueFromColor (CommonColor color)
		=> (double)color.A;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateHSB (alpha: (byte)Clamp (value));
	}

	class HlsHueChannelEditor : ChannelEditor
	{
		public HlsHueChannelEditor () : base ("H", 0d, 360d, 1d)
		{
		}

		public override double ValueFromColor (CommonColor color)
		=> color.Hue;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateHLS (hue: Clamp (value));
	}

	class HlsLightnessChannelEditor : ChannelEditor
	{
		public HlsLightnessChannelEditor () : base ("L", 0d, 1d, .001d)
		{
		}

		public override double ValueFromColor (CommonColor color)
		=> (double)color.Lightness;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateHLS (lightness: Clamp (value));
	}

	class HlsSaturationChannelEditor : ChannelEditor
	{
		public HlsSaturationChannelEditor () : base ("S", 0d, 1d, .001d)
		{
		}

		public override double ValueFromColor (CommonColor color)
		=> (double)color.Saturation;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateHLS (saturation: Clamp (value));
	}

	class HlsAlphaChannelEditor : ChannelEditor
	{
		public HlsAlphaChannelEditor () : base ("A", 0d, 255d, 1d)
		{
		}

		public override double ValueFromColor (CommonColor color)
		=> (double)color.A;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateHLS (alpha: (byte)Clamp (value));
	}
}
