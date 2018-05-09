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
			get => grip.BorderColor;
			set => grip.BorderColor = value;
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
			hueEditor.UpdateGradientLayer (colors, new CommonColor (0, 255, 0));
			AddSublayer (colors);
			AddSublayer (grip);
		}

		readonly CAGradientLayer colors = new CAGradientLayer {
			BorderColor = new CGColor (.5f, .5f, .5f, .5f),
			StartPoint = new CGPoint (0, 1),
			EndPoint = new CGPoint (0, 0),
			BorderWidth = 1,
			CornerRadius = BorderRadius,
		};

		readonly CALayer grip = new CALayerQuick {
			BorderColor = NSColor.Text.CGColor,
			BorderWidth = 2,
			CornerRadius = GripRadius,
		};

		CommonColor c;
		public override void UpdateFromModel (EditorInteraction interaction)
		{
			LayoutIfNeeded ();

			var color = interaction.Color;
			
			var loc = hueEditor.LocationFromColor (colors, color);
			grip.Frame = new CGRect (1, loc.Y - grip.Frame.Height / 2f, grip.Frame.Width, grip.Frame.Height);
		}

		public override void UpdateFromLocation (EditorInteraction interaction, CGPoint location)
		{
			var clos = Math.Min (colors.Frame.Height, Math.Max (0, location.Y - colors.Frame.Y));

			grip.Frame = new CGRect (
				1,
				clos + colors.Frame.Y - grip.Frame.Height / 2f,
				Frame.Width - 2,
				2 * GripRadius);

			if (interaction == null)
				return;

			var color = interaction.Color;
			c = interaction.Color = hueEditor.UpdateColorFromLocation (
				colors,
				color,
				location);
		}

        public override void Commit(EditorInteraction viewModel)
        {
			viewModel.ViewModel.CommitLastColor ();
        }

        public override void LayoutSublayers ()
		{
			base.LayoutSublayers ();
			colors.Frame = Bounds.Inset (2, 2);
			grip.Frame = new CGRect (
				grip.Frame.X,
				grip.Frame.Y,
				Frame.Width - 2,
				2 * GripRadius);
		}
	}
}
