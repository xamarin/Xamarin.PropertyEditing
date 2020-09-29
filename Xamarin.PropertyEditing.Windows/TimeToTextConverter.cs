using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using Xamarin.PropertyEditing.Common;

namespace Xamarin.PropertyEditing.Windows
{
	[ValueConversion (typeof (Time), typeof (string))]
	internal class TimeToTextConverter : MarkupExtension, IValueConverter
	{
		public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
			=> !(value is Time timeValue) ? DependencyProperty.UnsetValue
				: timeValue.ToString ();

		public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
			=> !(value is string timeValue) ? DependencyProperty.UnsetValue
				: Time.Parse (timeValue);

		public override object ProvideValue (IServiceProvider serviceProvider) => this;
	}
}
