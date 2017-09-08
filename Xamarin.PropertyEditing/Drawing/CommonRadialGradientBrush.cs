using System.Collections.Generic;

namespace Xamarin.PropertyEditing.Drawing
{
	/// <summary>
	/// Represents a radial gradient in the property editor panel.
	/// A focal point defines the beginning of the gradient, and a circle defines the end point of the gradient.
	/// </summary>
	public class CommonRadialGradientBrush : CommonGradientBrush
	{
		public CommonRadialGradientBrush(IEnumerable<CommonGradientStop> stops)
			: base(stops) { }

		/// <summary>
		/// Gets or sets the center of the outermost circle of the radial gradient.
		/// </summary>
		public CommonPoint Center { get; set; }
		/// <summary>
		/// Gets or sets the location of the two-dimensional focal point that defines the beginning of the gradient.
		/// </summary>
		public CommonPoint GradientOrigin { get; set; }
		/// <summary>
		/// Gets or sets the horizontal radius of the outermost circle of the radial gradient.
		/// </summary>
		public double RadiusX { get; set; }
		/// <summary>
		/// Gets or sets the vertical radius of the outermost circle of the radial gradient.
		/// </summary>
		public double RadiusY { get; set; }
	}
}
