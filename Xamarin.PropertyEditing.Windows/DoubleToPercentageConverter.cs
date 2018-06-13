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
			=> !(value is double doubleValue) ? DependencyProperty.UnsetValue
				: doubleValue < 0 ? 0
				: doubleValue > 1 ? 100
				: doubleValue * 100;

		public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
			=> !(value is double doubleValue) ? DependencyProperty.UnsetValue
				: doubleValue < 0 ? 0
				: doubleValue > 100 ? 1
				: doubleValue / 100;

		public override object ProvideValue (IServiceProvider serviceProvider) => this;
	}
}
