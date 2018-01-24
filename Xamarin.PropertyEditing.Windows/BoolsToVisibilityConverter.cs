using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Xamarin.PropertyEditing.Windows
{
	internal class BoolsToVisibilityConverter
		: IMultiValueConverter
	{
		public object Convert (object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			bool value = false;
			if (values != null) {
				value = (bool)values[0];
				for (int i = 1; i < values.Length; i++) {
					value = value && (bool)values[i];
				}
			}

			return (value) ? Visibility.Visible : Visibility.Collapsed;
		}

		public object[] ConvertBack (object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException ();
		}
	}
}