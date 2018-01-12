using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Xamarin.PropertyEditing.Windows
{
	[ValueConversion (typeof (ValueInfo<string>), typeof (string))]
	internal class StringValueInfoConverter : MarkupExtension, IValueConverter
	{
		public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is string) && !(value is ValueInfo<string>)) return DependencyProperty.UnsetValue;

			if (value is ValueInfo<string> stringValueInfo) return stringValueInfo.Value;
			return (string)value;
		}

		public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is string stringValue)) return DependencyProperty.UnsetValue;

			return new ValueInfo<string> {
				Source = ValueSource.Local,
				Value = stringValue
			};
		}

		public override object ProvideValue (IServiceProvider serviceProvider) => this;
	}
}
