using System;
using CoreAnimation;
using CoreGraphics;

namespace Xamarin.PropertyEditing.Mac
{
	internal class HistoryLayer : ColorEditorLayer
	{
		private const float Margin = 3;
		private const float BorderRadius = 3;

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

		private readonly CALayer previous = new UnanimatedLayer ();
		private readonly CALayer current = new UnanimatedLayer ();
		private readonly CALayer last = new UnanimatedLayer ();

		private readonly CALayer lastClip = new UnanimatedLayer {
			BorderWidth = 1,
			CornerRadius = BorderRadius,
			BorderColor = new CGColor (.5f, .5f, .5f, .5f),
			MasksToBounds = true
		};

		private readonly CALayer clip = new UnanimatedLayer {
			BorderWidth = 1,
			CornerRadius = BorderRadius,
			BorderColor = new CGColor (.5f, .5f, .5f, .5f),
			MasksToBounds = true,
		};

		public override void LayoutSublayers ()
		{
			base.LayoutSublayers ();

			this.lastClip.Frame = new CGRect (
				Bounds.Right - Bounds.Height,
				0,
				Bounds.Height,
				Bounds.Height).Inset (Margin, Margin);

			this.clip.Frame = new CGRect (
				0,
				0,
				Bounds.Width - Bounds.Height + Margin,
				Bounds.Height).Inset (Margin, Margin);

			this.clip.Contents = DrawingExtensions.GenerateCheckerboard (this.clip.Bounds);
			this.lastClip.Contents = DrawingExtensions.GenerateCheckerboard (this.last.Bounds);
			this.last.Frame = this.lastClip.Bounds;

			var width = clip.Frame.Width / 2;
			previous.Frame = new CGRect (0, 0, width, this.clip.Frame.Height);
			current.Frame = new CGRect (width, 0, width, this.clip.Frame.Height);
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
