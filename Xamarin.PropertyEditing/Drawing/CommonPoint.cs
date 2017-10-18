using System;

namespace Xamarin.PropertyEditing.Drawing
{
	/// <summary>
	/// A point in two-dimensional space.
	/// </summary>
	[Serializable]
	public struct CommonPoint : IEquatable<CommonPoint>
	{
		/// <param name="x">The horizontal coordinate of the point.</param>
		/// <param name="y">The vertical coordinate of the point.</param>
		public CommonPoint(double x, double y)
		{
			X = x;
			Y = y;
		}

		/// <summary>
		/// The horizontal coordinate of the point.
		/// </summary>
		public double X { get; set; }
		/// <summary>
		/// The vertical coordinate of the point.
		/// </summary>
		public double Y { get; set; }

		public override bool Equals (object obj)
		{
			if (obj == null) return false;
			if (!(obj is CommonPoint)) return false;
			return Equals ((CommonPoint)obj);
		}

		public bool Equals (CommonPoint other)
		{
			return X == other.X &&
				   Y == other.Y;
		}

		public override int GetHashCode ()
		{
			var hashCode = 1861411795;
			unchecked {
				hashCode = hashCode * -1521134295 + X.GetHashCode ();
				hashCode = hashCode * -1521134295 + Y.GetHashCode ();
			}
			return hashCode;
		}
	}
}
