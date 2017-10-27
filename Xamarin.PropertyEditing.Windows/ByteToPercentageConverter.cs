using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Xamarin.PropertyEditing.Windows
{
	[ValueConversion (typeof (byte), typeof (string))]
	internal class ByteToPercentageConverter : MarkupExtension, IValueConverter
	{
		public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is byte)) return "100%";

			var alpha = (byte)value;
			return (alpha / 255d).ToString ("P0");
		}

		public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
		{
			var stringValue = value as string;
			if (string.IsNullOrWhiteSpace (stringValue)) return DependencyProperty.UnsetValue;
			stringValue = stringValue.TrimEnd (' ', '%');
			if (double.TryParse (stringValue, out double doubleValue)) {
				if (doubleValue < 0) return (byte)0;
				if (doubleValue > 100) return (byte)255;
				return (byte)(doubleValue * 2.55);
			}
			return DependencyProperty.UnsetValue;
		}

		public override object ProvideValue (IServiceProvider serviceProvider)
		{
			return this;
		}
	}
}
