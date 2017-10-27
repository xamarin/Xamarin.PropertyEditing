using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Xamarin.PropertyEditing.Windows
{
	[ValueConversion (typeof (double), typeof (string))]
	internal class DoubleToPercentageConverter : MarkupExtension, IValueConverter
	{
		public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is double)) return "100%";

			var doubleValue = (double)value;
			return doubleValue.ToString ("P1");
		}

		public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
		{
			var stringValue = value as string;
			if (string.IsNullOrWhiteSpace (stringValue)) return DependencyProperty.UnsetValue;
			stringValue = stringValue.TrimEnd (' ', '%');
			if (double.TryParse (stringValue, out double doubleValue)) {
				if (doubleValue < 0) return 0;
				if (doubleValue > 100) return 1;
				return doubleValue / 100;
			}
			return DependencyProperty.UnsetValue;
		}

		public override object ProvideValue (IServiceProvider serviceProvider)
		{
			return this;
		}
	}
}
