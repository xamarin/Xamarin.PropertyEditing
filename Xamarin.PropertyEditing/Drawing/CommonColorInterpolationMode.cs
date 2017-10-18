using System;

namespace Xamarin.PropertyEditing.Drawing
{
	/// <summary>
	/// Determines how the colors in a gradient are interpolated.
	/// </summary>
	[Serializable]
	public enum CommonColorInterpolationMode
	{
		/// <summary>
		/// Colors are interpolated in the scRGB color space.
		/// </summary>
		ScRgbLinearInterpolation,
		/// <summary>
		/// Colors are interpolated in the sRGB color space.
		/// </summary>
		SRgbLinearInterpolation
	}
}
