using System.Windows.Media;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.Windows
{
	internal static class ColorHelper
	{
		public static Color ToColor (this CommonColor commonColor)
			=> Color.FromArgb (commonColor.A, commonColor.R, commonColor.G, commonColor.B);

		public static CommonColor ToCommonColor (this Color color)
			=> new CommonColor (color.R, color.G, color.B, color.A);
	}
}
