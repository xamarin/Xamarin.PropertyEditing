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
			=> !(value is string dateValue) ? DependencyProperty.UnsetValue
				: Date.Parse (dateValue);

		public override object ProvideValue (IServiceProvider serviceProvider) => this;
	}
}
