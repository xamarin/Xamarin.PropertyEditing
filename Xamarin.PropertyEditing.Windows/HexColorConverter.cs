using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.Windows
{
	[ValueConversion (typeof (CommonColor), typeof (string))]
	internal class HexColorConverter : MarkupExtension, IValueConverter
	{
		public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is CommonColor)) return "#FF000000";

			var color = (CommonColor)value;
			return $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
		}

		public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
		{
			var stringValue = value as string;
			if (string.IsNullOrWhiteSpace (stringValue)) return DependencyProperty.UnsetValue;
			var color = (Color)ColorConverter.ConvertFromString (stringValue);
			return new CommonColor (color.R, color.G, color.B, color.A);
		}

		public override object ProvideValue (IServiceProvider serviceProvider)
		{
			return this;
		}
	}
}
