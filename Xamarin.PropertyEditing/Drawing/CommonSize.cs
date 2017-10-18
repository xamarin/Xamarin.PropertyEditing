using System;

namespace Xamarin.PropertyEditing.Drawing
{
	/// <summary>
	/// A size in two-dimensional space.
	/// </summary>
	[Serializable]
	public struct CommonSize : IEquatable<CommonSize>
	{
		public CommonSize(double width, double height)
		{
			Width = width;
			Height = height;
		}

		/// <summary>
		/// The width.
		/// </summary>
		public double Width { get; set; }
		/// <summary>
		/// The height.
		/// </summary>
		public double Height { get; set; }

		public override bool Equals (object obj)
		{
			if (obj == null) return false;
			if (!(obj is CommonSize)) return false;
			return Equals ((CommonSize)obj);
		}

		public bool Equals (CommonSize other)
		{
			return Width == other.Width &&
				   Height == other.Height;
		}

		public override int GetHashCode ()
		{
			var hashCode = 1861411795;
			unchecked {
				hashCode = hashCode * -1521134295 + Width.GetHashCode ();
				hashCode = hashCode * -1521134295 + Height.GetHashCode ();
			}
			return hashCode;
		}
	}
}
