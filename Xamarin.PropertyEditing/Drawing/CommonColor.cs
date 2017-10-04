using System;

namespace Xamarin.PropertyEditing.Drawing
{
	/// <summary>
	/// Describes a color.
	/// </summary>
	public struct CommonColor : IEquatable<CommonColor>
	{
		public CommonColor (byte r, byte g, byte b, byte a = 255)
		{
			A = a;
			R = r;
			G = g;
			B = b;
		}

		/// <summary>
		/// Alpha channel
		/// </summary>
		public byte A { get; set; }
		/// <summary>
		/// Red component
		/// </summary>
		public byte R { get; set; }
		/// <summary>
		/// Green component
		/// </summary>
		public byte G { get; set; }
		/// <summary>
		/// Blue component
		/// </summary>
		public byte B { get; set; }

		public override bool Equals (object obj)
		{
			if (obj == null) return false;
			if (!(obj is CommonThickness)) return false;
			return base.Equals ((CommonThickness)obj);
		}

		public bool Equals (CommonColor other)
		{
			return A == other.A &&
				   R == other.R &&
				   G == other.G &&
				   B == other.B;
		}

		public override int GetHashCode ()
		{
			var hashCode = 466501756;
			unchecked {
				hashCode = hashCode * -1521134295 + A.GetHashCode ();
				hashCode = hashCode * -1521134295 + R.GetHashCode ();
				hashCode = hashCode * -1521134295 + G.GetHashCode ();
				hashCode = hashCode * -1521134295 + B.GetHashCode ();
			}
			return hashCode;
		}
	}
}
