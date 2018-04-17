using System.Windows;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.Windows
{
	internal static class RectHelper
	{
		public static Rect ToRect (this CommonRectangle rectangle)
			=> new Rect {
				X = rectangle.X,
				Y = rectangle.Y,
				Width = rectangle.Width,
				Height = rectangle.Height
			};

		public static CommonRectangle ToCommonRectangle (this Rect rect)
			=> new CommonRectangle (
				x: rect.X,
				y: rect.Y,
				width: rect.Width,
				height: rect.Height
			);
	}
}
