using System;

namespace Xamarin.PropertyEditing.Drawing
{
	/// <summary>
	/// Specifies the coordinate system used by a brush.
	/// </summary>
	[Serializable]
	public enum CommonBrushMappingMode
	{
		/// <summary>
		/// The coordinate system is not relative to a bounding box.
		/// </summary>
		Absolute = 0,
		/// <summary>
		/// The coordinate system is relative to a bounding box.
		/// </summary>
		RelativeToBoundingBox = 1
	}
}
