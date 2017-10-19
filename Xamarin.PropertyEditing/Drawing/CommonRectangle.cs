using System;

namespace Xamarin.PropertyEditing.Drawing
{
	/// <summary>
	/// Describes the width, height, and location of a rectangle.
	/// </summary>
	public struct CommonRectangle : IEquatable<CommonRectangle>
	{
		/// <param name="x">The horizontal coordinate of left border of the rectangle.</param>
		/// <param name="y">The vertical coordinate of the top border of the rectangle.</param>
		/// <param name="width">The width of the rectangle.</param>
		/// <param name="height">The height of the rectangle.</param>
		public CommonRectangle (double x, double y, double width, double height)
		{
			X = x;
			Y = y;
			Size = new CommonSize (width, height);
		}

		/// <summary>
		/// The horizontal coordinate of left border of the rectangle.
		/// </summary>
		public double X { get; }
		/// <summary>
		/// The vertical coordinate of the top border of the rectangle.
		/// </summary>
		public double Y { get; }

		/// <summary>
		/// The width of the rectangle.
		/// </summary>
		public double Width => Size.Width;

		/// <summary>
		/// The height of the rectangle.
		/// </summary>
		public double Height => Size.Height;

		public CommonSize Size
		{
			get;
		}

		public override bool Equals (object obj)
		{
			if (obj == null) return false;
			if (!(obj is CommonRectangle)) return false;
			return base.Equals ((CommonRectangle)obj);
		}

		public bool Equals (CommonRectangle other)
		{
			return X == other.X &&
				   Y == other.Y &&
				   Width == other.Width &&
				   Height == other.Height;
		}

		public override int GetHashCode ()
		{
			var hashCode = 466501756;
			unchecked {
				hashCode = hashCode * -1521134295 + X.GetHashCode ();
				hashCode = hashCode * -1521134295 + Y.GetHashCode ();
				hashCode = hashCode * -1521134295 + Width.GetHashCode ();
				hashCode = hashCode * -1521134295 + Height.GetHashCode ();
			}
			return hashCode;
		}
	}
}
