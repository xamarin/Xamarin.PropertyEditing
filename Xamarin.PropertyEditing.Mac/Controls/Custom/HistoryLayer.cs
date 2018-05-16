using System;
using CoreAnimation;
using CoreGraphics;

namespace Xamarin.PropertyEditing.Mac
{
	class HistoryLayer : ColorEditorLayer
	{
		const float Margin = 3;
		const float BorderRadius = 3;

		public HistoryLayer ()
		{
			clip.AddSublayer (previous);
			clip.AddSublayer (current);
			AddSublayer (clip);
			AddSublayer (lastClip);
			lastClip.AddSublayer (last);
		}

		public HistoryLayer (IntPtr handle) : base (handle)
		{
		}

		readonly CALayer previous = new CALayerQuick ();
		readonly CALayer current = new CALayerQuick ();
		readonly CALayer last = new CALayerQuick ();

		readonly CALayer lastClip = new CALayer {
			BorderWidth = 1,
			CornerRadius = BorderRadius,
			BorderColor = new CGColor (.5f, .5f, .5f, .5f),
			MasksToBounds = true
		};

		readonly CALayer clip = new CALayer {
			BorderWidth = 1,
			CornerRadius = BorderRadius,
			BorderColor = new CGColor (.5f, .5f, .5f, .5f),
			MasksToBounds = true,
		};

		public override void LayoutSublayers ()
		{
			base.LayoutSublayers ();

			lastClip.Frame = new CGRect (
				Bounds.Right - Bounds.Height,
				0,
				Bounds.Height,
				Bounds.Height).Inset (Margin, Margin);

			clip.Frame = new CGRect (
				0,
				0,
				Bounds.Width - Bounds.Height + Margin,
				Bounds.Height).Inset (Margin, Margin);

			clip.Contents = DrawingExtensions.GenerateCheckerboard (clip.Bounds);
			lastClip.Contents = DrawingExtensions.GenerateCheckerboard (last.Bounds);
			last.Frame = lastClip.Bounds;

			var width = clip.Frame.Width / 2;
			previous.Frame = new CGRect (0, 0, width, clip.Frame.Height);
			current.Frame = new CGRect (width, 0, width, clip.Frame.Height);
		}

		public override void UpdateFromModel (EditorInteraction interaction)
		{
			LayoutIfNeeded ();
			current.BackgroundColor = interaction.Color.ToCGColor ();
			previous.BackgroundColor = interaction.ViewModel.InitialColor.ToCGColor ();
			last.BackgroundColor = interaction.LastColor.ToCGColor ();
		}

		public override void UpdateFromLocation (EditorInteraction interaction, CGPoint location)
		{
			if (previous == HitTest (location))
				interaction.Color = interaction.ViewModel.InitialColor;
		}

		public override void Commit (EditorInteraction interaction)
		{
		}
	}
}
