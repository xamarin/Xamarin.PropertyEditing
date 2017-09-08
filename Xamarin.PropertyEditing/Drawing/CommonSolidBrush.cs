using System.Drawing;

namespace Xamarin.PropertyEditing.Drawing
{
	/// <summary>
	/// Paints an area with a solid color.
	/// </summary>
	public class CommonSolidBrush : CommonBrush
	{
		public CommonSolidBrush(Color color)
		{
			Color = color;
		}

		/// <summary>
		/// The color of the brush.
		/// </summary>
		public Color Color { get; }
	}
}
