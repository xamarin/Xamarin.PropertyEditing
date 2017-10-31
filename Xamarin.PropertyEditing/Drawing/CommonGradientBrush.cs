using System;
using System.Collections.Generic;
using System.Linq;

namespace Xamarin.PropertyEditing.Drawing
{
	/// <summary>
	/// An abstract description of a gradient brush, composed of gradient stops.
	/// </summary>
	[Serializable]
	public abstract class CommonGradientBrush : CommonBrush
	{
		protected CommonGradientBrush (
			IEnumerable<CommonGradientStop> stops,
			CommonColorInterpolationMode colorInterpolationMode = CommonColorInterpolationMode.SRgbLinearInterpolation,
			CommonBrushMappingMode mappingMode = CommonBrushMappingMode.RelativeToBoundingBox,
			CommonGradientSpreadMethod spreadMethod = CommonGradientSpreadMethod.Pad,
			double opacity = 1.0)
			: base(opacity)
		{
			if (stops == null) {
				throw new ArgumentNullException (nameof (stops));
			}

			GradientStops = stops.ToArray ();
			ColorInterpolationMode = colorInterpolationMode;
			MappingMode = mappingMode;
			SpreadMethod = spreadMethod;
		}

		/// <summary>
		/// The brush's gradient stops.
		/// </summary>
		public IReadOnlyList<CommonGradientStop> GradientStops { get; }
		/// <summary>
		/// How the gradient's colors are interpolated.
		/// </summary>
		public CommonColorInterpolationMode ColorInterpolationMode { get; }
		/// <summary>
		/// Specifies whether the gradient brush's positioning coordinates are
		/// absolute or relative to the output area.
		/// </summary>
		public CommonBrushMappingMode MappingMode { get; }
		/// <summary>
		/// Specifies how to draw a gradient that starts or ends inside the
		/// bounds of the object to be painted.
		/// </summary>
		public CommonGradientSpreadMethod SpreadMethod { get; }

		public override bool Equals (object obj)
		{
			var brush = obj as CommonGradientBrush;
			if (brush == null) return false;
			return Equals (brush);
		}

		protected bool Equals (CommonGradientBrush other)
		{
			if (other == null) return false;
			if (GradientStops.Count != other.GradientStops.Count) return false;
			for(var i = 0; i < GradientStops.Count; i++) {
				if (GradientStops[i] != other.GradientStops[i]) return false;
			}
			return base.Equals (other) &&
				   ColorInterpolationMode == other.ColorInterpolationMode &&
				   MappingMode == other.MappingMode &&
				   SpreadMethod == other.SpreadMethod;
		}

		public override int GetHashCode ()
		{
			var hashCode = base.GetHashCode ();
			unchecked {
				foreach(var stop in GradientStops) {
					hashCode = hashCode * -1521134295 + stop.GetHashCode();
				}
				hashCode = hashCode * -1521134295 + ColorInterpolationMode.GetHashCode ();
				hashCode = hashCode * -1521134295 + MappingMode.GetHashCode ();
				hashCode = hashCode * -1521134295 + SpreadMethod.GetHashCode ();
			}
			return hashCode;
		}
	}
}
