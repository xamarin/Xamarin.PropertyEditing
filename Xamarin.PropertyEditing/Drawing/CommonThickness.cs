using System;

namespace Xamarin.PropertyEditing.Drawing
{
	/// <summary>
	/// A two-dimensional thickness, such as the thickness of the border of a rectangle.
	/// </summary>
	public struct CommonThickness : IEquatable<CommonThickness>
	{
		public CommonThickness(double uniformThickness)
			: this(uniformThickness, uniformThickness, uniformThickness, uniformThickness) { }

		public CommonThickness (double bottom, double left, double right, double top)
		{
			Bottom = bottom;
			Left = left;
			Right = right;
			Top = top;
		}

		/// <summary>
		/// The bottom thickness.
		/// </summary>
		public double Bottom { get; set; }
		/// <summary>
		/// The left thickness.
		/// </summary>
		public double Left { get; set; }
		/// <summary>
		/// The right thickness.
		/// </summary>
		public double Right { get; set; }
		/// <summary>
		/// The top thickness.
		/// </summary>
		public double Top { get; set; }

		public override bool Equals (object obj)
		{
			if (obj == null) return false;
			if (!(obj is CommonThickness)) return false;
			return base.Equals ((CommonThickness)obj);
		}

		public bool Equals (CommonThickness other)
		{
			return Bottom == other.Bottom &&
				   Left == other.Left &&
				   Right == other.Right &&
				   Top == other.Top;
		}

		public override int GetHashCode ()
		{
			var hashCode = 466501756;
			unchecked {
				hashCode = hashCode * -1521134295 + Bottom.GetHashCode ();
				hashCode = hashCode * -1521134295 + Left.GetHashCode ();
				hashCode = hashCode * -1521134295 + Right.GetHashCode ();
				hashCode = hashCode * -1521134295 + Top.GetHashCode ();
			}
			return hashCode;
		}
	}
}
