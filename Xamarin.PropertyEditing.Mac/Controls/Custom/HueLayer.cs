using System;
using AppKit;
using CoreAnimation;
using CoreGraphics;
using ObjCRuntime;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.Mac
{
	class HueLayer : ColorEditorLayer
	{
		private const float BorderRadius = 3;
		private const float GripRadius = 3;

		private readonly ChannelEditor hueEditor = new HsbHueChannelEditor ();

		public CGColor GripColor {
			get => grip.BorderColor;
			set => grip.BorderColor = value;
		}

		public HueLayer ()
		{
			Initialize ();
		}

		public HueLayer (NativeHandle handle) : base (handle)
		{
		}

		private void Initialize ()
		{
			this.hueEditor.UpdateGradientLayer (this.colors, new CommonColor (0, 255, 0));
			AddSublayer (colors);
			AddSublayer (grip);
		}

		private readonly CAGradientLayer colors = new CAGradientLayer {
			BorderColor = new CGColor (.5f, .5f, .5f, .5f),
			StartPoint = new CGPoint (0, 1),
			EndPoint = new CGPoint (0, 0),
			BorderWidth = 1,
			CornerRadius = BorderRadius,
		};

		private readonly CALayer grip = new UnanimatedLayer {
			BorderColor = NSColor.Text.CGColor,
			BorderWidth = 2,
			CornerRadius = GripRadius,
		};

		public override void UpdateFromModel (EditorInteraction interaction)
		{
			LayoutIfNeeded ();

			var color = interaction.Color;

			var loc = this.hueEditor.LocationFromColor (this.colors, color);
			this.grip.Frame = new CGRect (1, loc.Y - this.grip.Frame.Height / 2f, this.Frame.Width, this.grip.Frame.Height);
		}

		public override void UpdateFromLocation (EditorInteraction interaction, CGPoint location)
		{
			var clos = Math.Min (this.colors.Frame.Height, Math.Max (0, location.Y - this.colors.Frame.Y));

			this.grip.Frame = new CGRect (
				1,
				clos + this.colors.Frame.Y - this.grip.Frame.Height * 0.5f,
				Frame.Width - 2,
				2 * GripRadius);

			if (interaction == null)
				return;

			var color = interaction.Color;
			interaction.Color = this.hueEditor.UpdateColorFromLocation (
				this.colors,
				color,
				location);
		}

		public override void Commit (EditorInteraction interaction)
		{
			interaction.ViewModel.CommitLastColor ();
		}

		public override void LayoutSublayers ()
		{
			base.LayoutSublayers ();
			this.colors.Frame = Bounds;
			this.grip.Frame = new CGRect (
				this.grip.Frame.X,
				this.grip.Frame.Y,
				Frame.Width - 2,
				2 * GripRadius);
		}
	}
}
