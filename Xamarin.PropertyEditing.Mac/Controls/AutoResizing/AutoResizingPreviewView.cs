using System;
using System.Drawing;
using AppKit;
using CoreGraphics;
using Foundation;
using Xamarin.PropertyEditing.Common;

namespace Xamarin.PropertyEditing.Mac
{
	internal class AutoResizingPreviewView : NSView
	{
		private const int Height = 70;
		private NSImage originalBackgroundImage;
		private NSImage bgImage;
		private CGSize bgImageSize;

		private readonly NSColor backgroundFillColor = AutoResizingMaskView.backgroundFillColor;
		private readonly NSColor windowBorderColor = NSColor.FromDeviceRgba (.75f, .75f, .75f, 1);
		private readonly NSColor windowFillColor = NSColor.White;
		private readonly NSColor previewBorderColor = NSColor.FromDeviceRgba (.33f, .33f, .33f, 1);
		private readonly NSColor enabledElementFillColor = AutoResizingMaskView.activeArrowFillColor;
		private readonly NSColor disabledElementFillColor = AutoResizingMaskView.disabledArrowFillColor;

		private AutoResizingFlags mask;
		private IDisposable currentAnimation;
		private int currentAnimationRawValue;
		private int currentAnimationValue;

		private CGRect lastBounds;
		private NSTrackingArea trackArea;
		private bool enabled;

		public AutoResizingFlags Mask
		{
			get { return this.mask; }
			set
			{
				this.mask = value;
				NeedsDisplay = true;
			}
		}

		public AutoResizingPreviewView (NSImage backgroundImage)
		{
			this.originalBackgroundImage = backgroundImage;
			WantsLayer = true;
			this.trackArea = new NSTrackingArea (Frame, NSTrackingAreaOptions.MouseEnteredAndExited | NSTrackingAreaOptions.ActiveInKeyWindow, this, null);
			AddTrackingArea (this.trackArea);

			AppearanceChanged ();
		}

		private void AppearanceChanged ()
		{
			// Place holder so we can handle them changes
		}

		#region Overrriden Methods and Properties

		public override CGSize IntrinsicContentSize
		{
			get { return new CGSize ((int)(Height * originalBackgroundImage.Size.Width / originalBackgroundImage.Size.Height), Height + 6); }
		}

		public override bool IsFlipped
		{
			get { return true; }
		}

		public override void UpdateTrackingAreas ()
		{
			base.UpdateTrackingAreas ();
			RemoveTrackingArea (trackArea);
			this.trackArea = new NSTrackingArea (new CGRect (CGPoint.Empty, Frame.Size), NSTrackingAreaOptions.MouseEnteredAndExited | NSTrackingAreaOptions.ActiveInKeyWindow, this, null);
			AddTrackingArea (this.trackArea);
		}

		public override void DrawRect (CGRect dirtyRect)
		{
			var rect = Bounds;
			UpdateBackgroundSurface (rect);
			DrawPreviewBackground (rect);

			var x = (int)((rect.Width - bgImageSize.Width) / 2);
			var y = (int)((rect.Height - bgImageSize.Height) / 2);

			// Overlay interface
			var size = GetWindowSize (bgImageSize);
			CGRect windowRect = new CGRect (x + 10, y + 10, size.Width, size.Height);

			var elementRect = GetElementRectForWindowAndMask (new RectangleF ((float)windowRect.X, (float)windowRect.Y, (float)windowRect.Width, (float)windowRect.Height), this.mask);

			this.windowFillColor.Set ();
			NSGraphics.RectFill (windowRect);
			this.windowBorderColor.Set ();
			NSGraphics.FrameRectWithWidth (windowRect, 1);

			if (this.enabled)
				this.enabledElementFillColor.Set ();
			else
				this.disabledElementFillColor.Set ();
			NSGraphics.RectFill (new CGRect (elementRect.X, elementRect.Y, elementRect.Size.Width, elementRect.Size.Height));
		}

		public override void MouseEntered (NSEvent theEvent)
		{
			base.MouseEntered (theEvent);
			if (this.enabled)
				StartAnimation ();
		}

		public override void MouseExited (NSEvent theEvent)
		{
			base.MouseExited (theEvent);
			if (this.enabled)
				StopAnimation ();
			NeedsDisplay = true;
		}

		public sealed override void ViewDidChangeEffectiveAppearance ()
		{
			base.ViewDidChangeEffectiveAppearance ();

			AppearanceChanged ();
		}
		#endregion

		public bool Enabled
		{
			get { return this.enabled; }

			internal set
			{
				if (this.enabled == value)
					return;

				this.enabled = value;

				// Our state has changed, repaint
				NeedsDisplay = true;
			}
		}

		private void UpdateBackgroundSurface (CGRect bounds)
		{
			if (bounds == this.lastBounds)
				return;

			var rect = bounds;

			this.bgImage = originalBackgroundImage;
			this.bgImage.Size = GetBoxSize (bgImage, rect.Width - 8, rect.Height - 8);
			this.bgImageSize = this.bgImage.Size;
			this.lastBounds = bounds;
		}

		private CGSize GetBoxSize (NSImage image, double maxWidth, double maxHeight)
		{
			var size = image.Size;
			var ratio = (nfloat)Math.Min (maxWidth / size.Width, maxHeight / size.Height);
			return new CGSize (size.Width * ratio, size.Height * ratio);
		}

		private void DrawPreviewBackground (CGRect bounds)
		{
			var rect = bounds;
			this.backgroundFillColor.Set ();
			NSGraphics.RectFill (rect);
			DrawingUtils.DrawLightShadedBezel (rect, flipped: IsFlipped);

			var x = (int)((rect.Width - bgImageSize.Width) / 2);
			var y = (int)((rect.Height - bgImageSize.Height) / 2);


			this.bgImage.Draw (new CGPoint (x, y), CGRect.Empty, NSCompositingOperation.SourceIn, 1);
			this.previewBorderColor.Set ();
			NSGraphics.FrameRectWithWidth (new CGRect (x, y, this.bgImage.Size.Width, this.bgImage.Size.Height), 1);
		}

		private void StartAnimation ()
		{
			if (this.currentAnimation != null)
				return;
			const int LowerBound = 90;
			const int UpperBound = 110;
			const int TimeStep = 50;

			this.currentAnimationRawValue = (UpperBound + LowerBound) / 2 - 1 - LowerBound;

			this.currentAnimation = NSTimer.CreateRepeatingScheduledTimer (TimeSpan.FromMilliseconds (TimeStep), _ => {
				// A oscillating function based on abs(x) which values goes linearly from LowerBound to UpperBound
				this.currentAnimationRawValue = ((this.currentAnimationRawValue + 1 + (UpperBound - LowerBound)) % (2 * UpperBound + 1 - 2 * LowerBound)) - (UpperBound - LowerBound);
				this.currentAnimationValue = Math.Abs (this.currentAnimationRawValue) + LowerBound;
				NeedsDisplay = true;
			});
		}

		private void StopAnimation ()
		{
			if (this.currentAnimation == null)
				return;
			this.currentAnimation.Dispose ();
			this.currentAnimation = null;
		}

		private Size GetWindowSize (CGSize canvasSize)
		{
			var baseWidth = (int)(canvasSize.Width / 2);
			var baseHeight = (int)(canvasSize.Height / 2);

			if (this.currentAnimation != null) {
				var adder = this.currentAnimationValue - 100;
				baseWidth += adder;
				baseHeight += adder;
			}

			return new Size (baseWidth, baseHeight);
		}

		public static RectangleF GetElementRectForWindowAndMask (RectangleF window, AutoResizingFlags mask)
		{
			const int Offset = 5;
			var baseHeight = 10.0;
			if (mask.HasFlag (AutoResizingFlags.FlexibleDimensions) || mask.HasFlag (AutoResizingFlags.FlexibleHeight)) {
				baseHeight = window.Size.Height / 2.0;
				if (!mask.HasFlag (AutoResizingFlags.FlexibleTopMargin) && !mask.HasFlag (AutoResizingFlags.FlexibleMargins))
					baseHeight += window.Size.Height / 4.0 - Offset;
				if (!mask.HasFlag (AutoResizingFlags.FlexibleBottomMargin) && !mask.HasFlag (AutoResizingFlags.FlexibleMargins))
					baseHeight += window.Size.Height / 4.0 - Offset;
			}

			var baseWidth = 10.0;
			if (mask.HasFlag (AutoResizingFlags.FlexibleDimensions) || mask.HasFlag (AutoResizingFlags.FlexibleWidth)) {
				baseWidth = window.Size.Width / 2.0;
				if (!mask.HasFlag (AutoResizingFlags.FlexibleLeftMargin) && !mask.HasFlag (AutoResizingFlags.FlexibleMargins))
					baseWidth += window.Size.Width / 4.0 - Offset;
				if (!mask.HasFlag (AutoResizingFlags.FlexibleRightMargin) && !mask.HasFlag (AutoResizingFlags.FlexibleMargins))
					baseWidth += window.Size.Width / 4.0 - Offset;
			}

			double left = Offset;
			if (mask.HasFlag (AutoResizingFlags.FlexibleLeftMargin) || mask.HasFlag (AutoResizingFlags.FlexibleMargins)) {
				if (mask.HasFlag (AutoResizingFlags.FlexibleRightMargin) || mask.HasFlag (AutoResizingFlags.FlexibleMargins))
					left = (window.Size.Width - baseWidth) / 2.0;
				else
					left = window.Size.Width - Offset - baseWidth;
			}

			double top = Offset;
			if (mask.HasFlag (AutoResizingFlags.FlexibleTopMargin) || mask.HasFlag (AutoResizingFlags.FlexibleMargins)) {
				if (mask.HasFlag (AutoResizingFlags.FlexibleBottomMargin) || mask.HasFlag (AutoResizingFlags.FlexibleMargins))
					top = (window.Size.Height - baseHeight) / 2.0;
				else
					top = window.Size.Height - Offset - baseHeight;
			}

			return new RectangleF ((float)(window.X + left),
								  (float)(window.Y + top),
								  (float)(baseWidth),
								  (float)(baseHeight));
		}
	}
}
