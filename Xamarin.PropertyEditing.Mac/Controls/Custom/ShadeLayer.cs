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
			AddSublayer (saturationLayer);
			saturationLayer.AddSublayer (brightnessLayer);
			AddSublayer (grip);
			float innerRadius = GripRadius - 1;

			grip.AddSublayer (new CALayer {
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

		readonly CALayer grip = new CALayerQuick {
			BorderColor = new CGColor (1, 1, 1),
			BorderWidth = 1,
			CornerRadius = GripRadius,
		};

		readonly CAGradientLayer brightnessLayer = new CAGradientLayerQuick {
			Colors = new [] {
					new CGColor (0f, 0f, 0f, 1f),
					new CGColor (0f, 0f, 0f, 0f)
				},
			CornerRadius = BorderRadius,
		};

		readonly CAGradientLayer saturationLayer = new CAGradientLayerQuick {
			Colors = new [] {
					new CGColor (1f, 1f, 1f),
					new CGColor (1f, .3f, 0f)
				},
			Actions = new Foundation.NSDictionary (),
			BackgroundColor = NSColor.Black.CGColor,
			BorderColor = new CGColor (.5f, .5f, .5f, .5f),
			BorderWidth = 1,
			CornerRadius = BorderRadius,
		};

		public override void LayoutSublayers ()
		{
			base.LayoutSublayers ();

			saturationLayer.Frame = Bounds.Inset (Margin, Margin);
			brightnessLayer.Frame = saturationLayer.Bounds;
			saturationLayer.StartPoint = new CGPoint (0, .5);
			saturationLayer.EndPoint = new CGPoint (1, .5);
		}

		public override void UpdateFromModel (EditorInteraction interaction)
		{
			LayoutIfNeeded ();
			var color = interaction.Color;

			var sat = saturationEditor.LocationFromColor (saturationLayer, color);
			var bright = brightnessEditor.LocationFromColor (brightnessLayer, color);

			var x = sat.X;
			var y = bright.Y + saturationLayer.Frame.Y;

			grip.Frame = new CGRect (x - GripRadius, y - GripRadius, GripRadius * 2, GripRadius * 2);

			var hueColor = interaction.Color.HueColor;
			saturationEditor.UpdateGradientLayer (saturationLayer, hueColor);
		}

		public override void UpdateFromLocation (EditorInteraction interaction, CGPoint location)
		{
			var loc = location;
			var frame = saturationLayer.Frame;

			if (interaction.ViewModel == null)
				return;

			var color = interaction.Color;
			var saturation = saturationEditor.ValueFromLocation (saturationLayer, loc);
			var brightness = saturationEditor.ValueFromLocation (
				brightnessLayer,
				new CGPoint (loc.X + brightnessLayer.Frame.X, loc.Y + brightnessLayer.Frame.Y));

			interaction.Color = interaction.Color.UpdateHSB (saturation: saturation, brightness: brightness);
			//interaction.Shade = shade;
		}

		public override void Commit (EditorInteraction interaction)
		{
			interaction.ViewModel.CommitLastColor ();
		}
	}
}
