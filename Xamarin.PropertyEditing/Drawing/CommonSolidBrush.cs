using System;

namespace Xamarin.PropertyEditing.Drawing
{
	/// <summary>
	/// Paints an area with a solid color.
	/// </summary>
	[Serializable]
	public sealed class CommonSolidBrush : CommonBrush, IEquatable<CommonSolidBrush>
	{
		public CommonSolidBrush(CommonColor color, string colorSpace = null, double opacity = 1.0)
			: base(opacity)
		{
			Color = color;
			ColorSpace = colorSpace;
		}

		public CommonSolidBrush(byte r, byte g, byte b, byte a = 255, string colorSpace = null, double opacity = 1.0)
			: this(new CommonColor(r, g, b, a), colorSpace, opacity) { }

		/// <summary>
		/// The color of the brush.
		/// </summary>
		public CommonColor Color { get; }

		/// <summary>
		/// The color space the brush is defined in.
		/// </summary>
		public string ColorSpace { get; }

		public override bool Equals (object obj)
		{
			var brush = obj as CommonSolidBrush;
			if (brush == null) return false;
			return Equals(brush);
		}

		public bool Equals (CommonSolidBrush other)
		{
			return other != null &&
				   Color.Equals(other.Color) &&
				   ColorSpace == other.ColorSpace &&
				   Opacity == other.Opacity;
		}

		public static bool operator == (CommonSolidBrush left, CommonSolidBrush right) => Equals (left, right);
		public static bool operator != (CommonSolidBrush left, CommonSolidBrush right) => !Equals (left, right);

		public override int GetHashCode ()
		{
			var hashCode = base.GetHashCode ();
			unchecked {
				hashCode = hashCode * -1521134295 + Color.GetHashCode ();
				if (ColorSpace != null)
					hashCode = hashCode * -1521134295 + ColorSpace.GetHashCode ();
			}
			return hashCode;
		}
	}
}
