using System;
using System.Collections.Generic;
using System.Linq;
using CoreAnimation;
using CoreGraphics;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.Mac
{
	internal abstract class ChannelEditor
	{
		public string Name { get; }
		public double MinimumValue { get; }
		public double MaximumValue { get; }
		public double IncrementValue { get; }
		public string ToolTip { get; }

		static IEnumerable<double> LerpSteps (double min, double max, int steps)
			=> Enumerable.Range (0, steps).Select (v => {
				var pos = v / (double)steps;
				return max * pos - min * (1 - pos);
			});

		public ChannelEditor (string name, string toolTip, double min, double max, double increment)
		{
			MinimumValue = min;
			MaximumValue = max;
			IncrementValue = increment;
			Name = name;
			ToolTip = toolTip;
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

	internal class RedChannelEditor : ChannelEditor
	{
		public RedChannelEditor () : base ("R", Properties.Resources.Red, 0d, 255d, 1d)
		{
		}

		public override double ValueFromColor (CommonColor color)
		=> (double)color.R;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateRGB (r: (byte)Clamp (value));
	}

	internal class GreenChannelEditor : ChannelEditor
	{
		public GreenChannelEditor () : base ("G", Properties.Resources.Green, 0d, 255d, 1d)
		{
		}

		public override double ValueFromColor (CommonColor color)
		=> (double)color.G;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateRGB (g: (byte)Clamp (value));
	}

	internal class BlueChannelEditor : ChannelEditor
	{
		public BlueChannelEditor () : base ("B", Properties.Resources.Blue, 0d, 255d, 1d)
		{
		}

		public override double ValueFromColor (CommonColor color)
		=> (double)color.B;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateRGB (b: (byte)Clamp (value));
	}

	internal class AlphaChannelEditor : ChannelEditor
	{
		public AlphaChannelEditor () : base ("A", Properties.Resources.Alpha, 0d, 255d, 1d)
		{
		}

		public override double ValueFromColor (CommonColor color)
		=> (double)color.A;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateRGB (a: (byte)Clamp (value));
	}

	internal class CyanChannelEditor : ChannelEditor
	{
		public CyanChannelEditor () : base ("C", Properties.Resources.Cyan, 0d, 1d, .001d)
		{
		}

		public override double ValueFromColor (CommonColor color)
		=> color.C;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateCMYK (c: Clamp (value));
	}

	internal class MagentaChannelEditor : ChannelEditor
	{
		public MagentaChannelEditor () : base ("M", Properties.Resources.Magenta, 0d, 1d, .001d)
		{
		}

		public override double ValueFromColor (CommonColor color)
		=> color.M;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateCMYK (m: Clamp (value));
	}

	internal class YellowChannelEditor : ChannelEditor
	{
		public YellowChannelEditor () : base ("Y", Properties.Resources.Yellow, 0d, 1d, .001d)
		{
		}

		public override double ValueFromColor (CommonColor color)
		=> color.Y;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateCMYK (y: Clamp (value));
	}

	internal class BlackChannelEditor : ChannelEditor
	{
		public BlackChannelEditor () : base ("K", Properties.Resources.Black, 0d, 1d, .001d)
		{
		}

		public override double ValueFromColor (CommonColor color)
		=> color.K;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateCMYK (k: Clamp (value));
	}

	internal class HsbHueChannelEditor : ChannelEditor
	{
		public HsbHueChannelEditor () : base ("H", Properties.Resources.Hue, 0d, 360d, 1d)
		{
		}

		public override double ValueFromColor (CommonColor color)
		=> color.Hue;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateHSB (hue: Clamp (value));
	}

	internal class HsbSaturationChannelEditor : ChannelEditor
	{
		public HsbSaturationChannelEditor () : base ("S", Properties.Resources.Saturation, 0d, 1d, .001d)
		{
		}

		public override double ValueFromColor (CommonColor color)
		=> color.Saturation;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateHSB (saturation: Clamp (value));
	}

	internal class HsbBrightnessChannelEditor : ChannelEditor
	{
		public HsbBrightnessChannelEditor () : base ("B", Properties.Resources.Brightness, 0d, 1d, .001d)
		{
		}

		public override double ValueFromColor (CommonColor color)
		=> color.Brightness;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateHSB (brightness: Clamp (value));
	}

	internal class HsbAlphaChannelEditor : ChannelEditor
	{
		public HsbAlphaChannelEditor () : base ("A", Properties.Resources.Alpha, 0d, 255d, 1d)
		{
		}

		public override double ValueFromColor (CommonColor color)
		=> (double)color.A;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateHSB (alpha: (byte)Clamp (value));
	}

	internal class HlsHueChannelEditor : ChannelEditor
	{
		public HlsHueChannelEditor () : base ("H", Properties.Resources.Hue, 0d, 360d, 1d)
		{
		}

		public override double ValueFromColor (CommonColor color)
		=> color.Hue;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateHLS (hue: Clamp (value));
	}

	internal class HlsLightnessChannelEditor : ChannelEditor
	{
		public HlsLightnessChannelEditor () : base ("L", Properties.Resources.Lightness, 0d, 1d, .001d)
		{
		}

		public override double ValueFromColor (CommonColor color)
		=> (double)color.Lightness;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateHLS (lightness: Clamp (value));
	}

	internal class HlsSaturationChannelEditor : ChannelEditor
	{
		public HlsSaturationChannelEditor () : base ("S", Properties.Resources.Saturation, 0d, 1d, .001d)
		{
		}

		public override double ValueFromColor (CommonColor color)
		=> (double)color.Saturation;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateHLS (saturation: Clamp (value));
	}

	internal class HlsAlphaChannelEditor : ChannelEditor
	{
		public HlsAlphaChannelEditor () : base ("A", Properties.Resources.Alpha, 0d, 255d, 1d)
		{
		}

		public override double ValueFromColor (CommonColor color)
		=> (double)color.A;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateHLS (alpha: (byte)Clamp (value));
	}
}
