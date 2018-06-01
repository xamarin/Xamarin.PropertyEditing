using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Xamarin.PropertyEditing.Windows
{
	[ValueConversion (typeof (double), typeof (double))]
	internal class DoubleToAngleConverter : MarkupExtension, IValueConverter
	{
		public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
			=> !(value is double doubleValue) ? DependencyProperty.UnsetValue
				: doubleValue < 0 ? 0
				: doubleValue > 360 ? 360
				: doubleValue;

		public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
			=> Convert (value, targetType, parameter, culture);

		public override object ProvideValue (IServiceProvider serviceProvider) => this;
	}
}
