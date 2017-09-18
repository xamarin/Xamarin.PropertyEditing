using System;
using System.Collections.Generic;

namespace Xamarin.PropertyEditing.Drawing
{
	/// <summary>
	/// A linear gradient.
	/// </summary>
	public class CommonLinearGradientBrush : CommonGradientBrush, IEquatable<CommonLinearGradientBrush>
	{
		public CommonLinearGradientBrush (
			CommonPoint startPoint, CommonPoint endPoint,
			IEnumerable<CommonGradientStop> stops,
			CommonColorInterpolationMode colorInterpolationMode = CommonColorInterpolationMode.SRgbLinearInterpolation,
			CommonBrushMappingMode mappingMode = CommonBrushMappingMode.RelativeToBoundingBox,
			CommonGradientSpreadMethod spreadMethod = CommonGradientSpreadMethod.Pad,
			double opacity = 1.0)
			: base (stops, colorInterpolationMode, mappingMode, spreadMethod, opacity)
		{
			StartPoint = startPoint;
			EndPoint = endPoint;
		}

		/// <summary>
		/// The starting two-dimensional coordinates of the linear gradient.
		/// </summary>
		public CommonPoint StartPoint { get; }
		/// <summary>
		/// The ending two-dimensional coordinates of the linear gradient.
		/// </summary>
		public CommonPoint EndPoint { get; }

		public override bool Equals (object obj)
		{
			var brush = obj as CommonLinearGradientBrush;
			if (brush == null) return false;
			return Equals (brush);
		}

		public bool Equals (CommonLinearGradientBrush other)
		{
			return other != null &&
				   base.Equals (other) &&
				   StartPoint.Equals (other.StartPoint) &&
				   EndPoint.Equals (other.EndPoint);
		}

		public override int GetHashCode ()
		{
			var hashCode = base.GetHashCode ();
			unchecked {
				hashCode = hashCode * -1521134295 + StartPoint.GetHashCode ();
				hashCode = hashCode * -1521134295 + EndPoint.GetHashCode ();
			}
			return hashCode;
		}
	}
}
