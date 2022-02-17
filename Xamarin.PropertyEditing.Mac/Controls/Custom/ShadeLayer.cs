using System;
using AppKit;
using CoreAnimation;
using CoreGraphics;
using ObjCRuntime;

namespace Xamarin.PropertyEditing.Mac
{
	class ShadeLayer : ColorEditorLayer
	{
		private const float GripRadius = 4;
		private const float BorderRadius = 3;
		private readonly ChannelEditor saturationEditor = new HsbSaturationChannelEditor ();
		private readonly ChannelEditor brightnessEditor = new HsbBrightnessChannelEditor ();

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

		public ShadeLayer (NativeHandle handle) : base (handle)
		{
		}

		private readonly CALayer grip = new UnanimatedLayer {
			BorderColor = new CGColor (1, 1, 1),
			BorderWidth = 1,
			CornerRadius = GripRadius,
		};

		private readonly CAGradientLayer brightnessLayer = new UnanimatedGradientLayer {
			Colors = new [] {
					new CGColor (0f, 0f, 0f, 1f),
					new CGColor (0f, 0f, 0f, 0f)
				},
			CornerRadius = BorderRadius,
		};

		private readonly CAGradientLayer saturationLayer = new UnanimatedGradientLayer {
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

			this.saturationLayer.Frame = Bounds;
			this.brightnessLayer.Frame = this.saturationLayer.Bounds;
			this.saturationLayer.StartPoint = new CGPoint (0, .5);
			this.saturationLayer.EndPoint = new CGPoint (1, .5);
		}

		public override void UpdateFromModel (EditorInteraction interaction)
		{
			LayoutIfNeeded ();
			var color = interaction.Color;

			var sat = this.saturationEditor.LocationFromColor (this.saturationLayer, color);
			var bright = this.brightnessEditor.LocationFromColor (this.brightnessLayer, color);

			var x = sat.X;
			var y = bright.Y + this.saturationLayer.Frame.Y;

			grip.Frame = new CGRect (x - GripRadius, y - GripRadius, GripRadius * 2, GripRadius * 2);

			var hueColor = interaction.Color.HueColor;
			this.saturationEditor.UpdateGradientLayer (this.saturationLayer, hueColor);
		}

		public override void UpdateFromLocation (EditorInteraction interaction, CGPoint location)
		{
			var loc = location;
			var frame = saturationLayer.Frame;

			if (interaction.ViewModel == null)
				return;
			
			var color = interaction.Color;
			var saturation = this.saturationEditor.ValueFromLocation (this.saturationLayer, loc);
			var brightness = this.saturationEditor.ValueFromLocation (
				this.brightnessLayer,
				new CGPoint (loc.X + brightnessLayer.Frame.X, loc.Y + brightnessLayer.Frame.Y));
			
			interaction.Color = interaction.Color.UpdateHSB (saturation: saturation, brightness: brightness);
		}

		public override void Commit (EditorInteraction interaction)
		{
			interaction.ViewModel.CommitLastColor ();
		}
	}
}
