using System;
using System.Linq;
using AppKit;
using CoreGraphics;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.Mac
{
	internal class OriginControl : NSView, INSAccessibilityGroup
	{
		public event EventHandler OriginChanged;

		private CGPoint[] points;

		private NSColor innerFillColor = NSColor.White;
		private NSColor positionPointColor = NSColor.Black;
		private NSColor selectArrowColor = NSColor.Red;

		private nfloat[] lightBezelGreys = { .41f, .29f, .29f, .29f };

		protected UnfocusableTextField OriginLabel { get; set; }

		private CommonOrigin? origin;
		public CommonOrigin? Value {
			get { return this.origin; }

			set {
				if (this.origin.HasValue && this.origin.Value.Equals (value))
					return;

				this.origin = value;

				OriginChanged?.Invoke (this, EventArgs.Empty);
				NeedsDisplay = true;
			}
		}

		private bool enabled = true;

		public bool Enabled {
			get { return this.enabled; }

			internal set {
				if (this.enabled == value)
					return;

				this.enabled = value;

				// TODO get design input on colours
				if (this.enabled)
					this.selectArrowColor = NSColor.Red;
				else
					this.selectArrowColor = NSColor.SystemGrayColor;

				NeedsDisplay = true;
			}
		}

		private CGRect drawableRect;
		public override CGRect FocusRingMaskBounds => this.drawableRect;
		private readonly CommonOrigin[] originList = {
			CommonOrigin.TopLeft, CommonOrigin.TopMiddle, CommonOrigin.TopRight,
			CommonOrigin.MiddleLeft, CommonOrigin.Center, CommonOrigin.MiddleRight,
			CommonOrigin.BottomLeft, CommonOrigin.BottomMiddle, CommonOrigin.BottomRight
		};

		private readonly IHostResourceProvider hostResources;

		internal OriginControl (IHostResourceProvider hostResources)
		{
			if (hostResources == null)
				throw new ArgumentNullException (nameof (hostResources));

			this.hostResources = hostResources;

			OriginLabel = new UnfocusableTextField {
				Font = NSFont.FromFontName (PropertyEditorControl.DefaultFontName, PropertyEditorControl.DefaultDescriptionLabelFontSize),
				StringValue = "ORIGIN",
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			AddSubview (OriginLabel);

			AddConstraints (new[] {
				NSLayoutConstraint.Create (OriginLabel, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Bottom, 1f, 1f),
				NSLayoutConstraint.Create (OriginLabel, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 18),
				NSLayoutConstraint.Create (OriginLabel, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, this, NSLayoutAttribute.CenterX, 1f, 0),
			});

			AppearanceChanged ();
		}

#region overridden properties and methods
		public sealed override void ViewDidChangeEffectiveAppearance ()
		{
			base.ViewDidChangeEffectiveAppearance ();

			AppearanceChanged ();
		}

		public override bool BecomeFirstResponder ()
		{
			var willBecomeFirstResponder = base.BecomeFirstResponder ();
			if (willBecomeFirstResponder) {
				ScrollRectToVisible (Bounds);
			}

			return willBecomeFirstResponder;
		}

		public override bool CanBecomeKeyView { get { return Enabled; } }

		public override CGSize IntrinsicContentSize {
			get {
				return new CGSize (70, 70);
			}
		}

		public override bool IsFlipped {
			get {
				return true;
			}
		}

		public override void DrawFocusRingMask ()
		{
			NSGraphics.RectFill (this.drawableRect);
		}

		public override void DrawRect (CGRect dirtyRect)
		{
			const int rectRadius = 1;
			this.drawableRect = Bounds;
			this.drawableRect.Inflate (-rectRadius, -(6 + rectRadius));
			this.drawableRect.Y = 1;
			this.innerFillColor.Set ();
			NSGraphics.RectFill (this.drawableRect);

			NSGraphics.DrawTiledRects (this.drawableRect, this.drawableRect, new NSRectEdge[] {
					NSRectEdge.MinYEdge, NSRectEdge.MinXEdge, NSRectEdge.MaxXEdge, NSRectEdge.MaxYEdge
				},
				this.lightBezelGreys);

			var innerRect = this.drawableRect;
			innerRect.Inflate (-(5 + rectRadius), -(5 + rectRadius));

			// Draw focus points
			this.points = new CGPoint[] {
					new CGPoint (innerRect.Left, innerRect.Top),
					new CGPoint (innerRect.Center ().X, innerRect.Top),
					new CGPoint (innerRect.Right, innerRect.Top),
					new CGPoint (innerRect.Left, innerRect.Center ().Y),
					innerRect.Center (),
					new CGPoint (innerRect.Right, innerRect.Center ().Y),
					new CGPoint (innerRect.Left, innerRect.Bottom),
					new CGPoint (innerRect.Center ().X, innerRect.Bottom),
					new CGPoint (innerRect.Right, innerRect.Bottom)
				};

			this.positionPointColor.Set ();
			foreach (var point in this.points) {
				var path = new NSBezierPath ();
				path.AppendPathWithArc (point, rectRadius, 0, 360);
				path.Fill ();
			}

			// Draw selection arrows
			this.selectArrowColor.Set ();
			var center = GetRectCenterForOrigin (this.origin, innerRect);
			const int ArrowLength = 8;
			if (this.origin.HasValue) {
				foreach (var dir in this.origin.Value.GetArrowDirections ()) {
					switch (dir) {
						case CommonOrigin.Direction.Left:
							DrawArrow (center, new CGPoint (center.X - ArrowLength, center.Y));
							break;
						case CommonOrigin.Direction.Right:
							DrawArrow (center, new CGPoint (center.X + ArrowLength, center.Y));
							break;
						case CommonOrigin.Direction.Up:
							DrawArrow (center, new CGPoint (center.X, center.Y - ArrowLength));
							break;
						case CommonOrigin.Direction.Down:
							DrawArrow (center, new CGPoint (center.X, center.Y + ArrowLength));
							break;
					}
				}

			}
		}

		public override void KeyUp (NSEvent theEvent)
		{
			base.KeyUp (theEvent);

			if (!this.origin.HasValue)
				return;

			var currentOriginIndex = this.originList.IndexOf (this.origin.Value);

			switch (theEvent.KeyCode) {
				// Move to next left origin position
				case 123:
					currentOriginIndex--;
					if (currentOriginIndex < 0)
						currentOriginIndex = this.originList.Length - 1;

					Value = this.originList[currentOriginIndex];
					break;

				// Move to next right origin position
				case 124:
					currentOriginIndex++;
					if (currentOriginIndex > this.originList.Length - 1)
						currentOriginIndex = 0;

					Value = this.originList[currentOriginIndex];
					break;
			}
		}

		public override void MouseDown (NSEvent theEvent)
		{
			base.MouseDown (theEvent);

			if (this.points == null || !this.enabled)
				return;

			var mousePosition = ConvertPointFromView (Window.MouseLocationOutsideOfEventStream, null);
			var x = mousePosition.X;
			var y = mousePosition.Y;

			Func<CGPoint, double> distance = p => Math.Sqrt (Math.Pow (y - p.Y, 2) + Math.Pow (x - p.X, 2));
			var closestPoint = this.points.OrderBy (distance).First ();

			const int MinDistance = 5;
			if (distance (closestPoint) < MinDistance) {
				int index = this.points.IndexOf (closestPoint);
				Value = new CommonOrigin {
					Vertical = (CommonOrigin.Position)(index / 3),
					Horizontal = (CommonOrigin.Position)(index % 3)
				};
			}
		}
		#endregion

		#region Local Methods
		private void AppearanceChanged ()
		{
			if (EffectiveAppearance.Name.ToLower().Contains("dark")) {
				this.lightBezelGreys = new nfloat[] { .59f, .71f, .71f, .71f };
				this.positionPointColor = NSColor.DarkGray;
			} else {
				this.lightBezelGreys = new nfloat[] { .41f, .29f, .29f, .29f };
				this.positionPointColor = NSColor.LightGray;
			}

			this.innerFillColor = this.hostResources.GetNamedColor (NamedResources.ControlBackground);

			OriginLabel.TextColor = this.hostResources.GetNamedColor (NamedResources.DescriptionLabelColor);

			NeedsDisplay = true;
		}

		private void DrawArrow (CGPoint from, CGPoint to, int thickness = 1)
		{
			const int HeadOffset = 3;

			var path = new NSBezierPath ();
			// Draw body
			path.SetLineDash (new nfloat[] { 2, 1 }, 0);
			path.MoveTo (from);
			path.LineTo (to);
			path.Stroke ();

			path = new NSBezierPath ();
			// Draw head
			if (from.Y == to.Y) {
				// horizontal
				path.MoveTo (to);
				path.LineTo (new CGPoint ((from.X < to.X) ? to.X - HeadOffset : to.X + HeadOffset, to.Y + HeadOffset));
				path.MoveTo (to);
				path.LineTo (new CGPoint ((from.X < to.X) ? to.X - HeadOffset : to.X + HeadOffset, to.Y - HeadOffset));
				path.Stroke ();
			} else {
				// vertical
				path.MoveTo (to);
				path.LineTo (new CGPoint (to.X + HeadOffset, (from.Y < to.Y) ? to.Y - HeadOffset : to.Y + HeadOffset));
				path.MoveTo (to);
				path.LineTo (new CGPoint (to.X - HeadOffset, (from.Y < to.Y) ? to.Y - HeadOffset : to.Y + HeadOffset));
				path.Stroke ();
			}
		}
		#endregion

		private static CGPoint GetRectCenterForOrigin (CommonOrigin? origin, CGRect rect)
		{
			nfloat x = rect.Left;
			nfloat y = rect.Top;

			switch (origin?.Horizontal) {
				case CommonOrigin.Position.Middle:
					x = rect.Center ().X;
					break;

				case CommonOrigin.Position.End:
					x = rect.Right;
					break;
			}

			switch (origin?.Vertical) {
				case CommonOrigin.Position.Middle:
					y = rect.Center ().Y;
					break;

				case CommonOrigin.Position.End:
					y = rect.Bottom;
					break;
			}

			return new CGPoint (x, y);
		}

	}

	public static class CGRectEx
	{
		public static CGPoint Center (this CGRect rect)
		{
			return new CGPoint (rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
		}
	}
}
