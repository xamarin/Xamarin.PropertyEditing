using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Xamarin.PropertyEditing.Windows
{
	internal class MultiplyMarginConverter
		: IMultiValueConverter
	{
		public object Convert (object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			Thickness thickness = (Thickness)values[0];
			int by = (int)values[1];

			return new Thickness (thickness.Left * by, thickness.Top * by, thickness.Right * by, thickness.Bottom * by);
		}

		public object[] ConvertBack (object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException ();
		}
	}
}
