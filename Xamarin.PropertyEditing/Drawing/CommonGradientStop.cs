using System;

namespace Xamarin.PropertyEditing.Drawing
{
	/// <summary>
	/// Describes the location and color of a transition point in a gradient.
	/// </summary>
	public class CommonGradientStop : IEquatable<CommonGradientStop>
	{
		public CommonGradientStop (CommonColor color, double offset)
		{
			Color = color;
			Offset = offset;
		}

		/// <summary>
		/// Gets or sets the color of the gradient stop.
		/// </summary>
		public CommonColor Color { get; }

		/// <summary>
		/// Gets or sets the location of the gradient stop within the gradient vector.
		/// </summary>
		public double Offset { get; }

		public override bool Equals (object obj)
		{
			var stop = obj as CommonGradientStop;
			if (stop == null) return false;
			return Equals (stop);
		}

		public bool Equals (CommonGradientStop other)
		{
			return other != null &&
				   Color.Equals (other.Color) &&
				   Offset == other.Offset;
		}

		public override int GetHashCode ()
		{
			var hashCode = 1107944354;
			unchecked {
				hashCode = hashCode * -1521134295 + Color.GetHashCode ();
				hashCode = hashCode * -1521134295 + Offset.GetHashCode ();
			}
			return hashCode;
		}
	}
}
