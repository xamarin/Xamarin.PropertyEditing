using System;
using System.Globalization;
using System.Windows.Data;

namespace Xamarin.PropertyEditing.Windows
{
	internal class CategoryGroupConverter
		: IValueConverter
	{
		public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
		{
			string category = value as string;
			return String.IsNullOrWhiteSpace (category) ? "Miscellaneous" : value; // TODO: Localize
		}

		public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException ();
		}
	}
}