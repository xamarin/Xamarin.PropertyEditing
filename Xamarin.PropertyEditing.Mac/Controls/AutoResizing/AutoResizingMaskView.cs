using System;
using AppKit;
using CoreGraphics;
using Xamarin.PropertyEditing.Common;

namespace Xamarin.PropertyEditing.Mac
{
	internal class AutoResizingMaskView : NSView
	{
		public event EventHandler MaskChanged;

		private readonly NSColor internalBorderColor = NSColor.FromDeviceRgba (.54f, .54f, .54f, 1);
		internal static NSColor activeArrowFillColor = NSColor.FromDeviceRgba (.96f, .43f, .31f, 1);
		private readonly NSColor inactiveArrowFillColor = NSColor.FromDeviceRgba (.96f, .67f, .62f, 1);
		internal static NSColor disabledArrowFillColor = NSColor.FromDeviceRgba (.96f, .43f, .31f, 0.5f);
		internal static NSColor backgroundFillColor = NSColor.WindowBackground;

		private bool enabled;
		private AutoResizingFlags mask;
		private readonly AutoResizingFlags[] autoResizingFlagsList;

		public AutoResizingFlags Mask
		{
			get { return this.mask; }

			internal set
			{
				if (this.mask == value)
					return;

				this.mask = value;

				// Our state has changed, repaint
				MaskChanged?.Invoke (this, EventArgs.Empty);
				NeedsDisplay = true;
			}
		}

		#region Overrriden Methods and Properties

		public override CGSize IntrinsicContentSize
		{
			get { return new CGSize (76, 76); }
		}

		public override bool IsFlipped
		{
			get { return true; }
		}

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

		public override bool CanBecomeKeyView
		{
			get { return this.enabled; }
		}

		public override CGRect FocusRingMaskBounds => Bounds;

		public override bool AcceptsFirstResponder ()
		{
			return this.enabled;
		}

		public override void DrawRect (CGRect dirtyRect)
		{
			var rect = Bounds;
			backgroundFillColor.Set ();
			NSGraphics.RectFill (rect);
			DrawingUtils.DrawLightShadedBezel (rect, flipped: IsFlipped);

			// Draw inner rectangle
			var fourthWidth = (int)rect.Width / 4;
			var fourthHeight = (int)rect.Height / 4;
			var innerRect = new CGRect (fourthWidth, fourthHeight, 2 * fourthWidth, 2 * fourthHeight);
			backgroundFillColor.Set ();
			NSGraphics.RectFill (innerRect);
			this.internalBorderColor.Set ();
			NSGraphics.FrameRectWithWidth (innerRect, 1);

			const int ArrowOffset = 3;

			// Draw position arrows
			DrawPositionArrow (new CGPoint (ArrowOffset, 2 * fourthHeight),
								new CGPoint (fourthWidth - ArrowOffset, 2 * fourthHeight),
								!Mask.HasFlag (AutoResizingFlags.FlexibleLeftMargin) && !Mask.HasFlag (AutoResizingFlags.FlexibleMargins));
			DrawPositionArrow (new CGPoint (3 * fourthWidth + ArrowOffset - 1, 2 * fourthHeight),
								new CGPoint ((int)rect.Width - ArrowOffset - 1, 2 * fourthHeight),
								!Mask.HasFlag (AutoResizingFlags.FlexibleRightMargin) && !Mask.HasFlag (AutoResizingFlags.FlexibleMargins));
			DrawPositionArrow (new CGPoint (2 * fourthWidth, ArrowOffset),
								new CGPoint (2 * fourthWidth, fourthHeight - ArrowOffset),
								!Mask.HasFlag (AutoResizingFlags.FlexibleTopMargin) && !Mask.HasFlag (AutoResizingFlags.FlexibleMargins));
			DrawPositionArrow (new CGPoint (2 * fourthWidth, 3 * fourthHeight + ArrowOffset - 1),
								new CGPoint (2 * fourthWidth, (int)rect.Height - ArrowOffset - 1),
								!Mask.HasFlag (AutoResizingFlags.FlexibleBottomMargin) && !Mask.HasFlag (AutoResizingFlags.FlexibleMargins));

			// Draw size arrows
			DrawSizeArrow (new CGPoint (fourthWidth + ArrowOffset, 2 * fourthHeight), new CGPoint (3 * fourthWidth - ArrowOffset, 2 * fourthHeight),
							Mask.HasFlag (AutoResizingFlags.FlexibleWidth) || Mask.HasFlag (AutoResizingFlags.FlexibleDimensions));
			DrawSizeArrow (new CGPoint (2 * fourthWidth, fourthHeight + ArrowOffset), new CGPoint (2 * fourthWidth, 3 * fourthHeight - ArrowOffset),
							Mask.HasFlag (AutoResizingFlags.FlexibleHeight) || Mask.HasFlag (AutoResizingFlags.FlexibleDimensions));
		}

		public override void MouseDown (NSEvent theEvent)
		{
			if (!this.enabled)
				return;

			var mousePosition = ConvertPointFromView (Window.MouseLocationOutsideOfEventStream, null);
			var x = mousePosition.X;
			var y = mousePosition.Y;

			const int Tolerance = 5;
			var rect = Bounds;

			var inHorizontal = y > rect.Height / 2 - Tolerance && y < rect.Height / 2 + Tolerance;
			var inVertical = x > rect.Width / 2 - Tolerance && x < rect.Width / 2 + Tolerance;
			if (!inHorizontal && !inVertical)
				return;

			var fourthWidth = rect.Width / 4;
			var fourthHeight = rect.Height / 4;

			if (inHorizontal) {
				if (x < fourthWidth)
					ToggleMaskFlag (AutoResizingFlags.FlexibleLeftMargin);
				else if (x > 3 * fourthWidth)
					ToggleMaskFlag (AutoResizingFlags.FlexibleRightMargin);
				else
					ToggleMaskFlag (AutoResizingFlags.FlexibleWidth);
			} else {
				if (y < fourthHeight)
					ToggleMaskFlag (AutoResizingFlags.FlexibleTopMargin);
				else if (y > 3 * fourthHeight)
					ToggleMaskFlag (AutoResizingFlags.FlexibleBottomMargin);
				else
					ToggleMaskFlag (AutoResizingFlags.FlexibleHeight);
			}
		}

		public override void KeyUp (NSEvent theEvent)
		{
			if (!this.enabled)
				return;

			var currentOriginIndex = this.autoResizingFlagsList.IndexOf (this.mask);

			switch (theEvent.KeyCode) {
			// Move to next left flag position
			case 123:
				currentOriginIndex--;
				if (currentOriginIndex < 0)
					currentOriginIndex = this.autoResizingFlagsList.Length - 1;

				Mask = this.autoResizingFlagsList[currentOriginIndex];
				break;

			// Move to next right flag position
			case 124:
				currentOriginIndex++;
				if (currentOriginIndex > this.autoResizingFlagsList.Length - 1)
					currentOriginIndex = 0;

				Mask = this.autoResizingFlagsList[currentOriginIndex];
				break;

			default:
				base.KeyUp (theEvent);
				break;
			}
		}

		public override bool BecomeFirstResponder ()
		{
			var willBecomeFirstResponder = base.BecomeFirstResponder ();
			if (willBecomeFirstResponder) {
				ScrollRectToVisible (Bounds);
			}

			return willBecomeFirstResponder;
		}

		public override void DrawFocusRingMask ()
		{
			NSGraphics.RectFill (Bounds);
		}

		public sealed override void ViewDidChangeEffectiveAppearance ()
		{
			base.ViewDidChangeEffectiveAppearance ();

			AppearanceChanged ();
		}
		#endregion

		public AutoResizingMaskView ()
		{
			this.autoResizingFlagsList = (AutoResizingFlags[])Enum.GetValues (typeof (AutoResizingFlags));
			AppearanceChanged ();
		}

		private void DrawPositionArrow (CGPoint from, CGPoint to, bool full)
		{
			if (full && !this.enabled)
				disabledArrowFillColor.Set ();
			else if (full)
				activeArrowFillColor.Set ();
			else
				this.inactiveArrowFillColor.Set ();

			DrawUtils.DrawStraightLine (from, to, full ? null : new int[] { 0, 2, 2, 1, 4, 1, 2 });

			const int KnobLength = 5;

			Func<CGPoint, int, CGPoint> pointUpdater;
			if (from.Y == to.Y)
				pointUpdater = (p, m) => new CGPoint (p.X, p.Y + m * KnobLength + (1 - m) / 2);
			else
				pointUpdater = (p, m) => new CGPoint (p.X + m * KnobLength + (1 - m) / 2, p.Y);

			DrawUtils.DrawStraightLine (pointUpdater (from, -1), pointUpdater (from, 1), full ? null : new int[] { 0, 1 });
			DrawUtils.DrawStraightLine (pointUpdater (to, -1), pointUpdater (to, 1), full ? null : new int[] { 0, 1 });
		}

		private void DrawSizeArrow (CGPoint from, CGPoint to, bool full)
		{
			const int HeadOffset = 4;

			if (full && !this.enabled)
				disabledArrowFillColor.Set ();
			else if (full)
				activeArrowFillColor.Set ();
			else
				this.inactiveArrowFillColor.Set ();

			//var padding = from.Y == to.Y ? new Size (3, 0) : new Size (0, 3);
			DrawUtils.DrawStraightLine (from, to, full ? null : new int[] { 3, 1 });

			Action<CGPoint, CGPoint> drawHead;

			// Draw head
			if (from.Y == to.Y) {
				// horizontal
				drawHead = (f, t) => {
					var path = new NSBezierPath ();
					path.Append (new CGPoint[] {
						new CGPoint ((f.X < t.X) ? t.X - HeadOffset : t.X + HeadOffset, t.Y + HeadOffset + .5f),
						new CGPoint (t.X, t.Y + .5f),
						new CGPoint ((f.X < t.X) ? t.X - HeadOffset : t.X + HeadOffset, t.Y - HeadOffset + .5f),
					});
					path.Stroke ();
				};
			} else {
				// vertical
				drawHead = (f, t) => {
					var path = new NSBezierPath ();
					path.Append (new CGPoint[] {
						new CGPoint (t.X + HeadOffset + .5f, (f.Y < t.Y) ? t.Y - HeadOffset : t.Y + HeadOffset),
						new CGPoint (t.X + .5f, t.Y),
						new CGPoint (t.X - HeadOffset + .5f, (f.Y < t.Y) ? t.Y - HeadOffset : t.Y + HeadOffset)
					});
					path.Stroke ();
				};
			}

			drawHead (from, to);
			drawHead (to, from);
		}

		private void ToggleMaskFlag (AutoResizingFlags flag)
		{
			Mask ^= flag;
			MaskChanged?.Invoke (this, EventArgs.Empty);
			NeedsDisplay = true;
		}

		private void AppearanceChanged ()
		{
			// Placeholder to handle theme changes
			DrawingUtils.UpdateBezelGreys (EffectiveAppearance.Name.Contains ("dark")); // TODO temporary hack
		}
	}
}
