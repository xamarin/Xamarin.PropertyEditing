using System;
using System.Collections.Generic;
using System.Drawing;

namespace Xamarin.PropertyEditing.Drawing
{
	/// <summary>
	/// Paints an area with a solid color.
	/// </summary>
	public class CommonSolidBrush : CommonBrush, IEquatable<CommonSolidBrush>
	{
		public CommonSolidBrush(Color color, double opacity = 1.0)
			: base(opacity)
		{
			Color = color;
		}

		/// <summary>
		/// The color of the brush.
		/// </summary>
		public Color Color { get; }

		public override bool Equals (object obj)
		{
			var brush = obj as CommonSolidBrush;
			if (brush == null) return false;
			return Equals(brush);
		}

		public bool Equals (CommonSolidBrush other)
		{
			return other != null &&
				   EqualityComparer<Color>.Default.Equals (Color, other.Color) &&
				   Opacity == other.Opacity;
		}

		public override int GetHashCode ()
		{
			var hashCode = base.GetHashCode ();
			unchecked {
				hashCode = hashCode * -1521134295 + Color.GetHashCode ();
			}
			return hashCode;
		}
	}
}
