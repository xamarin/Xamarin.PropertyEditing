using System;
using System.Globalization;
using System.Windows.Data;

namespace Xamarin.PropertyEditing.Windows
{
	internal class OppositeBoolConverter
		: IValueConverter
	{
		public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null || !(value is bool))
				return null;

			return !(bool)value;
		}

		public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException ();
		}
	}
}