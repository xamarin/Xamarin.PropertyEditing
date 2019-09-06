using System;
using System.Collections.Generic;
using System.Drawing;

namespace Xamarin.PropertyEditing.Drawing
{
	public struct CommonOrigin : IEquatable<CommonOrigin>
	{
		public enum Position
		{
			Start = 0, // i.e. left or top
			Middle,
			End // i.e. right or bottom
		}

		public enum Direction
		{
			Up,
			Down,
			Left,
			Right
		}

		public IEnumerable<Direction> GetArrowDirections ()
		{
			switch (Horizontal) {
				case Position.Start:
					yield return Direction.Right;
					break;
				case Position.Middle:
					yield return Direction.Left;
					yield return Direction.Right;
					break;
				case Position.End:
					yield return Direction.Left;
					break;
			}

			switch (Vertical) {
				case Position.Start:
					yield return Direction.Down;
					break;
				case Position.Middle:
					yield return Direction.Up;
					yield return Direction.Down;
					break;
				case Position.End:
					yield return Direction.Up;
					break;
			}
		}

		public Point ToPoint (RectangleF rect)
		{
			int x = (int)rect.Left;
			int y = (int)rect.Top;

			switch (Horizontal) {
				case Position.Middle:
					x = (int)rect.Center ().X;
					break;
				case Position.End:
					x = (int)rect.Right;
					break;
			}

			switch (Vertical) {
				case Position.Middle:
					y = (int)rect.Center ().Y;
					break;
				case Position.End:
					y = (int)rect.Bottom;
					break;
			}

			return new Point (x, y);
		}

		public Position Horizontal { get; set; }
		public Position Vertical { get; set; }

		public static readonly CommonOrigin TopLeft = new CommonOrigin { Vertical = Position.Start, Horizontal = Position.Start };
		public static readonly CommonOrigin TopMiddle = new CommonOrigin { Vertical = Position.Start, Horizontal = Position.Middle };
		public static readonly CommonOrigin TopRight = new CommonOrigin { Vertical = Position.Start, Horizontal = Position.End };
		public static readonly CommonOrigin MiddleLeft = new CommonOrigin { Vertical = Position.Middle, Horizontal = Position.Start };
		public static readonly CommonOrigin Center = new CommonOrigin { Vertical = Position.Middle, Horizontal = Position.Middle };
		public static readonly CommonOrigin MiddleRight = new CommonOrigin { Vertical = Position.Middle, Horizontal = Position.End };
		public static readonly CommonOrigin BottomLeft = new CommonOrigin { Vertical = Position.End, Horizontal = Position.Start };
		public static readonly CommonOrigin BottomMiddle = new CommonOrigin { Vertical = Position.End, Horizontal = Position.Middle };
		public static readonly CommonOrigin BottomRight = new CommonOrigin { Vertical = Position.End, Horizontal = Position.End };

		public bool Equals (CommonOrigin other)
		{
			return other.Horizontal == Horizontal && other.Vertical == Vertical;
		}

		public override bool Equals (object obj)
		{
			return obj is CommonOrigin && Equals ((CommonOrigin)obj);
		}

		public override int GetHashCode ()
		{
			return Horizontal.GetHashCode () ^ Vertical.GetHashCode ();
		}
	}

	public static class PointEx
	{
		public static PointF Odd (this Point p)
		{
			return new PointF (p.X + .5f, p.Y + .5f);
		}
	}

	public static class RectEx
	{
		public static PointF Center (this RectangleF rect)
		{
			return new PointF (rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
		}

		public static PointF Center (this Rectangle rect)
		{
			return ((RectangleF)rect).Center ();
		}
	}
}
