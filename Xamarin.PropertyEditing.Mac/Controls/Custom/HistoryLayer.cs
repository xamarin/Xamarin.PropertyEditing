using System;
using AppKit;
using CoreAnimation;
using CoreGraphics;
using ObjCRuntime;

namespace Xamarin.PropertyEditing.Mac
{
	internal class HistoryLayer : ColorEditorLayer
	{
		private const float BorderRadius = 3;

		public HistoryLayer (IHostResourceProvider hostResources)
		{
			if (hostResources == null)
				throw new ArgumentNullException (nameof (hostResources));

			this.hostResources = hostResources;
			clip.AddSublayer (previous);
			clip.AddSublayer (current);
			AddSublayer (clip);
			AddSublayer (lastClip);
			lastClip.AddSublayer (last);
		}

		public HistoryLayer (NativeHandle handle) : base (handle)
		{
		}

		private readonly IHostResourceProvider hostResources;
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
				Bounds.Height);

			this.clip.Frame = new CGRect (
				0,
				0,
				Bounds.Width - Bounds.Height - 3,
				Bounds.Height);

			NSColor cc0 = this.hostResources.GetNamedColor (NamedResources.Checkerboard0Color);
			NSColor cc1 = this.hostResources.GetNamedColor (NamedResources.Checkerboard1Color);

			this.clip.Contents = DrawingExtensions.GenerateCheckerboard (this.clip.Bounds, cc0, cc1);
			this.lastClip.Contents = DrawingExtensions.GenerateCheckerboard (this.last.Bounds, cc0, cc1);
			this.last.Frame = this.lastClip.Bounds;

			var width = clip.Frame.Width / 2;
			previous.Frame = new CGRect (0, 0, width, this.clip.Frame.Height);
			current.Frame = new CGRect (width, 0, width, this.clip.Frame.Height);
		}

		public override void UpdateFromModel (EditorInteraction interaction)
		{
			LayoutIfNeeded ();
			current.BackgroundColor = interaction.Color.ToCGColor ();
			//there are some scenarios where viewmodel could be null
			var initialColor = interaction.ViewModel?.InitialColor;
			if (initialColor != null) {
				previous.BackgroundColor = initialColor.ToCGColor ();
			}
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
