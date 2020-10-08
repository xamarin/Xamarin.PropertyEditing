using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using Xamarin.PropertyEditing.Common;

namespace Xamarin.PropertyEditing.Windows
{
	[ValueConversion (typeof (Date), typeof (string))]
	internal class DateToTextConverter : MarkupExtension, IValueConverter
	{ 
		public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
			=> !(value is Date dateValue) ? DependencyProperty.UnsetValue
				: dateValue.ToString ();

		public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is string dateValue) {
				var parsedValue = Date.Parse (dateValue);
				if (parsedValue != null)
					return parsedValue;
				else
					return value;
			} else {
				return DependencyProperty.UnsetValue;
  			}
		}

		public override object ProvideValue (IServiceProvider serviceProvider) => this;
	}
}
