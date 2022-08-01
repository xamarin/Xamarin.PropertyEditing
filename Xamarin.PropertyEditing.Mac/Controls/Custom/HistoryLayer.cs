using System;
using AppKit;
using CoreAnimation;
using CoreGraphics;
using ObjCRuntime;
using Xamarin.PropertyEditing.Drawing;

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
			SetCALayerBackgroundColor (current, interaction.Color);
			//there are some scenarios where viewmodel could be null
			if (interaction.ViewModel != null)
				SetCALayerBackgroundColor (previous, interaction.ViewModel.InitialColor);
			SetCALayerBackgroundColor (last, interaction.LastColor);
		}

		static void SetCALayerBackgroundColor(CALayer layer, CommonColor color)
		{
			// We need to use GC.KeepAlive to ensure that the CGColor doesn't get garbage collected (and thus the
			// native object Released) before the BackgroundColor assignment is complete
			CGColor cgColor = color.ToCGColor ();
			layer.BackgroundColor = cgColor;
			GC.KeepAlive (cgColor);
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
