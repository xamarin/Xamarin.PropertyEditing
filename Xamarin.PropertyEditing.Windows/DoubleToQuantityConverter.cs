using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Xamarin.PropertyEditing.Windows
{
	[ValueConversion (typeof (double), typeof (string))]
	internal class DoubleToQuantityConverter : MarkupExtension, IMultiValueConverter
	{
		public object Convert (object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			var doubleValue = (double)values[0];
			var unit = (values.Length > 0 ? values[1] as string : "") ?? "";
			return $"{doubleValue:F0}{unit}";
		}

		public object[] ConvertBack (object value, Type[] targetTypes, object parameter, CultureInfo culture)
			=> throw new NotImplementedException ();

		public override object ProvideValue (IServiceProvider serviceProvider) => this;
	}
}
