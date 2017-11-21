using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Xamarin.PropertyEditing.Windows
{
	[ValueConversion (typeof (byte), typeof (double))]
	internal class ByteToPercentageConverter : MarkupExtension, IValueConverter
	{
		public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is byte)) return DependencyProperty.UnsetValue;

			var alpha = (byte)value;
			return (alpha / 2.55d);
		}

		public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is double doubleValue) {
				if ((doubleValue < 0) || (doubleValue > 100)) return DependencyProperty.UnsetValue;
				return System.Convert.ToByte (doubleValue * 2.55d);
			}
			return DependencyProperty.UnsetValue;
		}

		public override object ProvideValue (IServiceProvider serviceProvider) => this;
	}
}
