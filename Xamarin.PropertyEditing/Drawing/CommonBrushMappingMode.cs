namespace Xamarin.PropertyEditing.Drawing
{
	/// <summary>
	/// Specifies the coordinate system used by a brush.
	/// </summary>
	public enum CommonBrushMappingMode
	{
		/// <summary>
		/// The coordinate system is not relative to a bounding box.
		/// </summary>
		Absolute,
		/// <summary>
		/// The coordinate system is relative to a bounding box.
		/// </summary>
		RelativeToBoundingBox
	}
}
