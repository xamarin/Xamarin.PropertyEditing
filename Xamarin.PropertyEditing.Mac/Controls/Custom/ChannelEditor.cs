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
		public double Scale { get; }
		public string ToolTip { get; }
		public string FocusedFormat { get; }
		public string DisplayFormat { get; }

		static IEnumerable<double> LerpSteps (double min, double max, int steps)
			=> Enumerable.Range (0, steps).Select (v => {
				var pos = v / (double)steps;
				return max * pos - min * (1 - pos);
			});

		public ChannelEditor (string name, string toolTip, double min, double max, double increment, string focusedFormat = "0", string displayFormat = "0", double scale = 1d)
		{
			MinimumValue = min;
			MaximumValue = max;
			IncrementValue = increment;
			Scale = scale;
			Name = name;
			ToolTip = toolTip;
			FocusedFormat = focusedFormat;
			DisplayFormat = displayFormat;
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
			var pos = ValueFromColor (color) * Scale;

			var amount = InvLerp (MinimumValue, MaximumValue, pos);
			var unitLoc = Lerp (layer.StartPoint, layer.EndPoint, amount);

			return new CGPoint (
				layer.Frame.X + unitLoc.X * layer.Frame.Width,
				layer.Frame.Y + unitLoc.Y * layer.Frame.Height);
		}

		public double Clamp (double value)
		=> Math.Max (MinimumValue, Math.Min (MaximumValue, value)) / Scale;

		public abstract CommonColor UpdateColorFromValue (CommonColor color, double value);
		public abstract double ValueFromColor (CommonColor color);
	}

	internal abstract class ByteChannelEditor : ChannelEditor
	{
		public ByteChannelEditor (string name, string toolTip) : base (name, toolTip, 0d, 255d, 1d)
		{
		}
	}

	internal abstract class PercentageChannelEditor : ChannelEditor {
		public PercentageChannelEditor (string name, string toolTip) : base (name, toolTip, 0d, 100d, 1d, "0.#", "0'%'", 100d)
		{
		}
	}

	internal class RedChannelEditor : ByteChannelEditor
	{
		public RedChannelEditor () : base ("R", Properties.Resources.Red)
		{
		}

		public override double ValueFromColor (CommonColor color)
		=> (double)color.R;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateRGB (r: (byte)Clamp (value));
	}

	internal class GreenChannelEditor : ByteChannelEditor
	{
		public GreenChannelEditor () : base ("G", Properties.Resources.Green)
		{
		}

		public override double ValueFromColor (CommonColor color)
		=> (double)color.G;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateRGB (g: (byte)Clamp (value));
	}

	internal class BlueChannelEditor : ByteChannelEditor
	{
		public BlueChannelEditor () : base ("B", Properties.Resources.Blue)
		{
		}

		public override double ValueFromColor (CommonColor color)
		=> (double)color.B;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateRGB (b: (byte)Clamp (value));
	}

	internal class AlphaChannelEditor : ByteChannelEditor
	{
		public AlphaChannelEditor () : base ("A", Properties.Resources.Alpha)
		{
		}

		public override double ValueFromColor (CommonColor color)
		=> (double)color.A;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateRGB (a: (byte)Clamp (value));
	}

	internal class CyanChannelEditor : PercentageChannelEditor
	{
		public CyanChannelEditor () : base ("C", Properties.Resources.Cyan)
		{
		}

		public override double ValueFromColor (CommonColor color)
		=> color.C;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateCMYK (c: Clamp (value));
	}

	internal class MagentaChannelEditor : PercentageChannelEditor
	{
		public MagentaChannelEditor () : base ("M", Properties.Resources.Magenta)
		{
		}

		public override double ValueFromColor (CommonColor color)
		=> color.M;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateCMYK (m: Clamp (value));
	}

	internal class YellowChannelEditor : PercentageChannelEditor
	{
		public YellowChannelEditor () : base ("Y", Properties.Resources.Yellow)
		{
		}

		public override double ValueFromColor (CommonColor color)
		=> color.Y;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateCMYK (y: Clamp (value));
	}

	internal class BlackChannelEditor : PercentageChannelEditor
	{
		public BlackChannelEditor () : base ("K", Properties.Resources.Black)
		{
		}

		public override double ValueFromColor (CommonColor color)
		=> color.K;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateCMYK (k: Clamp (value));
	}

	internal class HsbHueChannelEditor : ChannelEditor
	{
		public HsbHueChannelEditor () : base ("H", Properties.Resources.Hue, 0d, 360d, 1d, "0.#", "0°")
		{
		}

		public override double ValueFromColor (CommonColor color)
		=> color.Hue;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateHSB (hue: Clamp (value));
	}

	internal class HsbSaturationChannelEditor : PercentageChannelEditor
	{
		public HsbSaturationChannelEditor () : base ("S", Properties.Resources.Saturation)
		{
		}

		public override double ValueFromColor (CommonColor color)
		=> color.Saturation;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateHSB (saturation: Clamp (value));
	}

	internal class HsbBrightnessChannelEditor : PercentageChannelEditor
	{
		public HsbBrightnessChannelEditor () : base ("B", Properties.Resources.Brightness)
		{
		}

		public override double ValueFromColor (CommonColor color)
		=> color.Brightness;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateHSB (brightness: Clamp (value));
	}

	internal class HsbAlphaChannelEditor : ByteChannelEditor
	{
		public HsbAlphaChannelEditor () : base ("A", Properties.Resources.Alpha)
		{
		}

		public override double ValueFromColor (CommonColor color)
		=> (double)color.A;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateHSB (alpha: (byte)Clamp (value));
	}

	internal class HlsHueChannelEditor : ChannelEditor
	{
		public HlsHueChannelEditor () : base ("H", Properties.Resources.Hue, 0d, 360d, 1d, "0.#", "0°")
		{
		}

		public override double ValueFromColor (CommonColor color)
		=> color.Hue;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateHLS (hue: Clamp (value));
	}

	internal class HlsLightnessChannelEditor : PercentageChannelEditor
	{
		public HlsLightnessChannelEditor () : base ("L", Properties.Resources.Lightness)
		{
		}

		public override double ValueFromColor (CommonColor color)
		=> (double)color.Lightness;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateHLS (lightness: Clamp (value));
	}

	internal class HlsSaturationChannelEditor : PercentageChannelEditor
	{
		public HlsSaturationChannelEditor () : base ("S", Properties.Resources.Saturation)
		{
		}

		public override double ValueFromColor (CommonColor color)
		=> (double)color.Saturation;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateHLS (saturation: Clamp (value));
	}

	internal class HlsAlphaChannelEditor : ByteChannelEditor
	{
		public HlsAlphaChannelEditor () : base ("A", Properties.Resources.Alpha)
		{
		}

		public override double ValueFromColor (CommonColor color)
		=> (double)color.A;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateHLS (alpha: (byte)Clamp (value));
	}
}
