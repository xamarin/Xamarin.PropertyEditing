using System;

namespace Xamarin.PropertyEditing.Drawing
{
	/// <summary>
	/// Describes the width, height, and location of a rectangle.
	/// </summary>
	[Serializable]
	public struct CommonRectangle : IEquatable<CommonRectangle>
	{
		/// <param name="x">The horizontal coordinate of left border of the rectangle.</param>
		/// <param name="y">The vertical coordinate of the top border of the rectangle.</param>
		/// <param name="width">The width of the rectangle.</param>
		/// <param name="height">The height of the rectangle.</param>
		/// <param name="origin">The origin of the rectangle.</param>
		public CommonRectangle (double x, double y, double width, double height, CommonOrigin? origin = null)
		{
			X = x;
			Y = y;
			Size = new CommonSize (width, height);
			Origin = origin;
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

		/// <summary>
		/// The size of the rectangle.
		/// </summary>
		public CommonSize Size { get; }

		/// <summary>
		/// Rectangle origin.
		/// </summary>
		public CommonOrigin? Origin { get; }

		public override bool Equals (object obj)
		{
			if (obj == null) return false;
			if (!(obj is CommonRectangle)) return false;
			return base.Equals ((CommonRectangle)obj);
		}

		public bool Equals (CommonRectangle other)
		{
			var isEqual = X == other.X &&
				   Y == other.Y &&
				   Width == other.Width &&
				   Height == other.Height;

			if (Origin.HasValue && other.Origin.HasValue)
				isEqual &= Origin.Value.Equals (other.Origin.Value);

			return isEqual;
		}

		public override int GetHashCode ()
		{
			var hashCode = 466501756;
			unchecked {
				hashCode = hashCode * -1521134295 + X.GetHashCode ();
				hashCode = hashCode * -1521134295 + Y.GetHashCode ();
				hashCode = hashCode * -1521134295 + Width.GetHashCode ();
				hashCode = hashCode * -1521134295 + Height.GetHashCode ();
				if (Origin.HasValue)
					hashCode = hashCode * -1521134295 + Origin.Value.GetHashCode ();
			}
			return hashCode;
		}
	}
}
