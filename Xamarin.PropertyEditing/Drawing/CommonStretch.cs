using System;

namespace Xamarin.PropertyEditing.Drawing
{
	/// <summary>
	/// Describes how content is resized to fill its allocated space.
	/// </summary>
	[Serializable]
	public enum CommonStretch
	{
		/// <summary>
		/// The content preserves its original size.
		/// </summary>
		None = 0,
		/// <summary>
		/// The content is resized to fill the destination dimensions. The aspect ratio is not preserved.
		/// </summary>
		Fill = 1,
		/// <summary>
		/// The content is resized to fit in the destination dimensions while it preserves its native aspect ratio.
		/// </summary>
		Uniform = 2,
		/// <summary>
		/// The content is resized to fill the destination dimensions while it preserves its native aspect ratio.
		/// If the aspect ratio of the destination rectangle differs from the source,
		/// the source content is clipped to fit in the destination dimensions.
		/// </summary>
		UniformToFill = 3
	}
}
