using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Xamarin.PropertyEditing.Windows
{
	[ValueConversion (typeof (object), typeof (string))]
	internal class ObjectToTypeNameConverter : MarkupExtension, IValueConverter
	{
		public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
		{
			switch (value) {
			case null:
				return "null";
			case Type type:
				return type.Name;
			default:
				return value.GetType ().Name;
			}
		}

		public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException ();
		}

		public override object ProvideValue (IServiceProvider serviceProvider) => this;
	}
}
