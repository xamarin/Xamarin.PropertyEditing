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
			Clip.AddSublayer (Previous);
			Clip.AddSublayer (Current);
			AddSublayer (Clip);
		}

		public HistoryLayer (IntPtr handle) : base (handle)
		{
		}

		readonly CALayer Previous = new CALayer ();
		readonly CALayer Current = new CALayer ();
		readonly CALayer Clip = new CALayer () {
			BorderWidth = 1,
			CornerRadius = BorderRadius,
			BorderColor = new CGColor (.5f, .5f, .5f, .5f),
			MasksToBounds = true,
		};

		public override void LayoutSublayers ()
		{
			base.LayoutSublayers ();

			Clip.Frame = new CGRect (
				Margin,
				Margin,
				Frame.Width - 2 * Margin,
				Frame.Height - 2 * Margin);

			Clip.Contents = DrawingExtensions.GenerateCheckerboard (Clip.Frame);
			var width = Clip.Frame.Width / 2;

			Previous.Frame = new CGRect (0, 0, width, Clip.Frame.Height);
			Current.Frame = new CGRect (width, 0, width, Clip.Frame.Height);
		}

		public override void UpdateFromModel (EditorInteraction interaction)
		{
			LayoutIfNeeded ();
			Current.BackgroundColor = interaction.Color.ToCGColor ();
			Previous.BackgroundColor = interaction.LastColor.ToCGColor ();
		}

		public override void UpdateFromLocation (EditorInteraction interaction, CGPoint location)
		{
			if (Previous == HitTest (location))
				interaction.Color = interaction.LastColor;
		}
	}
}
