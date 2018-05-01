using System;
using AppKit;
using CoreAnimation;
using CoreGraphics;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.Mac
{
	class HueLayer : ColorEditorLayer
	{
		const float BorderRadius = 3;
		const float GripRadius = 3;

		ChannelEditor hueEditor = new HsbHueChannelEditor ();

		public CGColor GripColor
		{
			get => Grip.BorderColor;
			set => Grip.BorderColor = value;
		}

		public HueLayer ()
		{
			Initialize ();
		}

		public HueLayer (IntPtr handle) : base (handle)
		{
		}

		void Initialize ()
		{
			hueEditor.UpdateGradientLayer (Colors, new CommonColor (0, 255, 0));
			AddSublayer (Colors);
			AddSublayer (Grip);
		}

		readonly CAGradientLayer Colors = new CAGradientLayer {
			BorderColor = new CGColor (.5f, .5f, .5f, .5f),
			StartPoint = new CGPoint (0, 1),
			EndPoint = new CGPoint (0, 0),
			BorderWidth = 1,
			CornerRadius = BorderRadius,
		};

		readonly CALayer Grip = new CALayer {
			BorderColor = NSColor.Text.CGColor,
			BorderWidth = 2,
			CornerRadius = GripRadius,
		};

		CommonColor c;
		public override void UpdateFromModel (EditorInteraction interaction)
		{
			LayoutIfNeeded ();

			var color = interaction.Color;
			if (c == color)
				return;

			var loc = hueEditor.LocationFromColor (Colors, color);
			Grip.Frame = new CGRect (1, loc.Y - Grip.Frame.Height / 2f, Grip.Frame.Width, Grip.Frame.Height);
		}

		public override void UpdateFromLocation (EditorInteraction interaction, CGPoint location)
		{
			var clos = Math.Min (Colors.Frame.Height, Math.Max (0, location.Y - Colors.Frame.Y));

			Grip.Frame = new CGRect (
				1,
				clos + Colors.Frame.Y - Grip.Frame.Height / 2f,
				Frame.Width - 2,
				2 * GripRadius);

			var hue = (1 - clos / Colors.Frame.Height) * 360;

			if (interaction == null)
				return;

			var color = interaction.Color;
			c = interaction.Color = hueEditor.UpdateColorFromLocation (
				Colors,
				interaction.Color,
				location);
		}

		public override void LayoutSublayers ()
		{
			base.LayoutSublayers ();
			Colors.Frame = Bounds.Inset (2, 2);
			Grip.Frame = new CGRect (
				Grip.Frame.X,
				Grip.Frame.Y,
				Frame.Width - 2,
				2 * GripRadius);
		}
	}
}
