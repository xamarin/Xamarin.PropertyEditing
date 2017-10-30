using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Xamarin.PropertyEditing.Windows
{
	[ValueConversion (typeof (double), typeof (string))]
	internal class DoubleToDegreesConverter : MarkupExtension, IValueConverter
	{
		public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
		{
			var doubleValue = (double)value;
			if (doubleValue == 1) doubleValue = 0;
			return $"{doubleValue:F1}°";
		}

		public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
		{
			var stringValue = value as string;
			if (string.IsNullOrWhiteSpace (stringValue)) return DependencyProperty.UnsetValue;
			stringValue = stringValue.TrimEnd (' ', '°');
			if (double.TryParse (stringValue, out double doubleValue)) {
				if (doubleValue < 0 || doubleValue > 360) return DependencyProperty.UnsetValue;
				return doubleValue;
			}
			return DependencyProperty.UnsetValue;
		}

		public override object ProvideValue (IServiceProvider serviceProvider)
		{
			return this;
		}
	}
}
