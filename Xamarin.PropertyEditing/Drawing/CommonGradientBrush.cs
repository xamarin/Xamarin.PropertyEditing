using System;
using System.Collections.Generic;
using System.Linq;

namespace Xamarin.PropertyEditing.Drawing
{
	/// <summary>
	/// An abstract description of a gradient brush, composed of gradient stops.
	/// </summary>
	public abstract class CommonGradientBrush : CommonBrush
	{
		protected CommonGradientBrush(IEnumerable<CommonGradientStop> stops)
		{
			if (stops == null) {
				throw new ArgumentNullException (nameof (stops));
			}

			GradientStops = stops.ToArray();
		}

		/// <summary>
		/// The brush's gradient stops.
		/// </summary>
		public IReadOnlyList<CommonGradientStop> GradientStops { get; }
		/// <summary>
		/// Gets or sets how the gradient's colors are interpolated.
		/// </summary>
		public CommonColorInterpolationMode ColorInterpolationMode { get; set; }
		/// <summary>
		/// Specifies whether the gradient brush's positioning coordinates are
		/// absolute or relative to the output area.
		/// </summary>
		public CommonBrushMappingMode MappingMode { get; set; }
		/// <summary>
		/// Specifies how to draw a gradient that starts or ends inside the
		/// bounds of the object to be painted.
		/// </summary>
		public CommonGradientSpreadMethod SpreadMethod { get; set; }
	}
}
