using System.Windows;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.Windows
{
	internal static class PointHelper
	{
		public static Point ToPoint (this CommonPoint point) => new Point(point.X, point.Y);
		public static CommonPoint ToCommonPoint (this Point point) => new CommonPoint (point.X, point.Y);
	}
}
