using System.Drawing;

namespace Xamarin.PropertyEditing.Drawing
{
	/// <summary>
	/// Describes the location and color of a transition point in a gradient.
	/// </summary>
	public class CommonGradientStop
	{
		public CommonGradientStop (Color color, double offset)
		{
			Color = color;
			Offset = offset;
		}

		/// <summary>
		/// Gets or sets the color of the gradient stop.
		/// </summary>
		public Color Color { get; set; }
		/// <summary>
		/// Gets or sets the location of the gradient stop within the gradient vector.
		/// </summary>
		public double Offset { get; set; }
	}
}
