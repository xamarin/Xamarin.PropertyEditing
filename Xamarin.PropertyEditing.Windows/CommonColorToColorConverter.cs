using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.Windows
{
	[ValueConversion (typeof (CommonColor), typeof (string))]
	public class CommonColorToColorConverter : MarkupExtension, IValueConverter
	{
		public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is CommonColor)) return Color.FromRgb(0, 0, 0);
			var commonColor = (CommonColor)value;
			return Color.FromArgb (commonColor.A, commonColor.R, commonColor.G, commonColor.B);
		}

		public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is Color)) return new CommonColor (0, 0, 0);
			var color = (Color)value;
			return new CommonColor (color.R, color.G, color.B, color.A);
		}

		public override object ProvideValue (IServiceProvider serviceProvider)
		{
			return this;
		}
	}
}
