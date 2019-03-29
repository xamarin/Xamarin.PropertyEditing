using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Xamarin.PropertyEditing.Windows
{
	/// <summary>
	/// Inverts a given thickness
	/// </summary>
	internal class NegativeThicknessConverter
		: IValueConverter
	{
		public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
		{
			Thickness t;
			if (value is Thickness thickness) {
				t = thickness;
			} else {
				t = new Thickness (0);
			}

			return new Thickness (t.Left * -1, t.Top * -1, t.Right * -1, t.Bottom * -1);
		}

		public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException ();
		}
	}
}