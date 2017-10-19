using System;
using System.Collections.Generic;

namespace Xamarin.PropertyEditing.Drawing
{
	/// <summary>
	/// Represents a radial gradient in the property editor panel.
	/// A focal point defines the beginning of the gradient, and a circle defines the end point of the gradient.
	/// </summary>
	[Serializable]
	public class CommonRadialGradientBrush : CommonGradientBrush, IEquatable<CommonRadialGradientBrush>
	{
		public CommonRadialGradientBrush(
			CommonPoint center,
			CommonPoint gradientOrigin,
			double radiusX,
			double radiusY,
			IEnumerable<CommonGradientStop> stops,
			CommonColorInterpolationMode colorInterpolationMode = CommonColorInterpolationMode.SRgbLinearInterpolation,
			CommonBrushMappingMode mappingMode = CommonBrushMappingMode.RelativeToBoundingBox,
			CommonGradientSpreadMethod spreadMethod = CommonGradientSpreadMethod.Pad,
			double opacity = 1.0)
			: base (stops, colorInterpolationMode, mappingMode, spreadMethod, opacity)
		{
			Center = center;
			GradientOrigin = gradientOrigin;
			RadiusX = radiusX;
			RadiusY = radiusY;
		}

		/// <summary>
		/// The center of the outermost circle of the radial gradient.
		/// </summary>
		public CommonPoint Center { get; }
		/// <summary>
		/// The location of the two-dimensional focal point that defines the beginning of the gradient.
		/// </summary>
		public CommonPoint GradientOrigin { get; }
		/// <summary>
		/// The horizontal radius of the outermost circle of the radial gradient.
		/// </summary>
		public double RadiusX { get; }
		/// <summary>
		/// The vertical radius of the outermost circle of the radial gradient.
		/// </summary>
		public double RadiusY { get; }

		public override bool Equals (object obj)
		{
			var brush = obj as CommonRadialGradientBrush;
			if (brush == null) return false;
			return Equals (brush);
		}

		public bool Equals (CommonRadialGradientBrush other)
		{
			return other != null &&
				   base.Equals (other) &&
				   Center.Equals (other.Center) &&
				   GradientOrigin.Equals (other.GradientOrigin) &&
				   RadiusX == other.RadiusX &&
				   RadiusY == other.RadiusY;
		}

		public static bool operator == (CommonRadialGradientBrush left, CommonRadialGradientBrush right) => Equals (left, right);
		public static bool operator != (CommonRadialGradientBrush left, CommonRadialGradientBrush right) => !Equals (left, right);

		public override int GetHashCode ()
		{
			var hashCode = base.GetHashCode ();
			unchecked {
				hashCode = hashCode * -1521134295 + Center.GetHashCode ();
				hashCode = hashCode * -1521134295 + GradientOrigin.GetHashCode ();
				hashCode = hashCode * -1521134295 + RadiusX.GetHashCode ();
				hashCode = hashCode * -1521134295 + RadiusY.GetHashCode ();
			}
			return hashCode;
		}
	}
}
