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
			if (values != null) {
				for (int i = 0; i < values.Length; i++) {
					object v = values[i];
					if (v == DependencyProperty.UnsetValue || !(bool) v)
						return Visibility.Collapsed;
				}
			}

			return Visibility.Visible;
		}

		public object[] ConvertBack (object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException ();
		}
	}
}