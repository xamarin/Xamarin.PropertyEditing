using System.Collections.Generic;

namespace Xamarin.PropertyEditing.Drawing
{
	/// <summary>
	/// A linear gradient.
	/// </summary>
	public class CommonLinearGradientBrush : CommonGradientBrush
	{
		public CommonLinearGradientBrush(IEnumerable<CommonGradientStop> stops)
			: base (stops) { }

		/// <summary>
		/// The starting two-dimensional coordinates of the linear gradient.
		/// </summary>
		public CommonPoint StartPoint { get; set; }
		/// <summary>
		/// The ending two-dimensional coordinates of the linear gradient.
		/// </summary>
		public CommonPoint EndPoint { get; set; }
	}
}
