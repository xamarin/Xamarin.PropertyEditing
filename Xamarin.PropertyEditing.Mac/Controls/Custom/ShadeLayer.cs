using System;
using System.Linq;
using AppKit;
using CoreAnimation;
using CoreGraphics;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.Mac
{
	class ShadeLayer : ColorEditorLayer
	{
		const float GripRadius = 4;
		const float BorderRadius = 3;
		const float Margin = 3;
		ChannelEditor saturationEditor = new HsbSaturationChannelEditor ();
		ChannelEditor brightnessEditor = new HsbBrightnessChannelEditor ();

		public ShadeLayer ()
		{
			AddSublayer (Saturation);
			Saturation.AddSublayer (Brightness);
			AddSublayer (Grip);
			float innerRadius = GripRadius - 1;

			Grip.AddSublayer (new CALayer {
				BorderWidth = 1,
				BorderColor = new CGColor (0, 0, 0),
				Frame = new CGRect (
					GripRadius - innerRadius,
					GripRadius - innerRadius,
					innerRadius * 2,
					innerRadius * 2),
				CornerRadius = innerRadius
			});
		}

		public ShadeLayer (IntPtr handle) : base (handle)
		{
		}

		CALayer Grip = new CALayer {
			BorderColor = new CGColor (1, 1, 1),
			BorderWidth = 1,
			CornerRadius = GripRadius,
		};

		CAGradientLayer Brightness = new CAGradientLayer {
			Colors = new[] {
					new CGColor (0f, 0f, 0f, 1f),
					new CGColor (0f, 0f, 0f, 0f)
				},
			CornerRadius = BorderRadius,
		};

		CAGradientLayer Saturation = new CAGradientLayer {
			Colors = new[] {
					new CGColor (1f, 1f, 1f),
					new CGColor (1f, .3f, 0f)
				},
			BackgroundColor = NSColor.Black.CGColor,
			BorderColor = new CGColor (.5f, .5f, .5f, .5f),
			BorderWidth = 1,
			CornerRadius = BorderRadius,
		};

		public override void LayoutSublayers ()
		{
			base.LayoutSublayers ();

			Saturation.Frame = new CGRect (Margin, Margin, Frame.Width - 2 * Margin, Frame.Height - 2 * Margin);
			Brightness.Frame = Saturation.Bounds;
			Saturation.StartPoint = new CGPoint (0, .5);
			Saturation.EndPoint = new CGPoint (1, .5);
		}

		CommonColor c;
		public override void UpdateFromModel (EditorInteraction interaction)
		{
			LayoutIfNeeded ();
			var color = interaction.Color;

			var sat = saturationEditor.LocationFromColor (Saturation, color);
			var bright = brightnessEditor.LocationFromColor (Brightness, color);

			var x = sat.X;
			var y = bright.Y + Saturation.Frame.Y;

			Grip.Frame = new CGRect (x - GripRadius, y - GripRadius, GripRadius * 2, GripRadius * 2);

			if (interaction.StartColor.ToCGColor () != Saturation.Colors.Last ())
				saturationEditor.UpdateGradientLayer (Saturation, interaction.StartColor.HueColor);
		}

		public override void UpdateFromLocation (EditorInteraction interaction, CGPoint location)
		{
			var loc = location;
			var frame = Saturation.Frame;

			if (interaction.ViewModel == null)
				return;

			var color = interaction.Color;
			var saturation = saturationEditor.ValueFromLocation (Saturation, loc);
			var brightness = saturationEditor.ValueFromLocation (
				Brightness,
				new CGPoint (loc.X + Brightness.Frame.X, loc.Y + Brightness.Frame.Y));

			c = interaction.Color = interaction.Color.UpdateHSB (saturation: saturation, brightness: brightness);
		}
	}
}
