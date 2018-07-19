using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Xamarin.PropertyEditing.Windows
{
	internal class InvertedVisibilityConverter
		: IValueConverter
	{
		public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
		{
			return ((Visibility) value == Visibility.Collapsed) ? Visibility.Visible : Visibility.Collapsed;
		}

		public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException ();
		}
	}
}
