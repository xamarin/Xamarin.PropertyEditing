using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Xamarin.PropertyEditing.Windows
{
	[ValueConversion (typeof (double), typeof (double))]
	internal class DoubleToPercentageConverter : MarkupExtension, IValueConverter
	{
		public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is double) && !(value is ValueInfo<double>)) return DependencyProperty.UnsetValue;

			double doubleValue;
			if (value is double) {
				doubleValue = (double)value;
			} else if (value is ValueInfo<double> doubleValueInfo && doubleValueInfo.Source == ValueSource.Local) {
				doubleValue = doubleValueInfo.Value;
			} else {
				return DependencyProperty.UnsetValue;
			}

			if (doubleValue< 0 || doubleValue> 1) return DependencyProperty.UnsetValue;
			return doubleValue* 100;
		}

		public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is double)) return DependencyProperty.UnsetValue;

			var doubleValue = (double)value;
			if ((doubleValue < 0) || (doubleValue > 100)) return DependencyProperty.UnsetValue;

			var convertedValue = doubleValue / 100;

			if (targetType.IsAssignableFrom (typeof (double))) return convertedValue;
			if (targetType.IsAssignableFrom (typeof (ValueInfo<double>))) return new ValueInfo<double> {
				Source = ValueSource.Local,
				Value = convertedValue
			};
			return DependencyProperty.UnsetValue;
		}

		public override object ProvideValue (IServiceProvider serviceProvider) => this;
	}
}
