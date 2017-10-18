using System;

namespace Xamarin.PropertyEditing.Drawing
{
	/// <summary>
	/// Specifies how to draw the gradient outside a gradient brush's gradient vector or space.
	/// </summary>
	[Serializable]
	public enum CommonGradientSpreadMethod
	{
		/// <summary>
		/// The color values at the end of the gradient vector fill the remaining space.
		/// </summary>
		Pad,
		/// <summary>
		/// The gradient is repeated in the reverse direction until the space is filled.
		/// </summary>
		Reflect,
		/// <summary>
		/// The gradient is repeated in the original direction until the space is filled.
		/// </summary>
		Repeat
	}
}
