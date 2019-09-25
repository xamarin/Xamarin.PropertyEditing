using System;
using System.Globalization;
using System.Windows.Data;
using Xamarin.PropertyEditing.Properties;

namespace Xamarin.PropertyEditing.Windows
{
	internal class ArrangeModeLocalizedConverter
		: IValueConverter
	{
		public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is PropertyArrangeMode mode) {
				switch (mode) {
				case PropertyArrangeMode.Category:
					return Resources.ArrangeByCategory;
				case PropertyArrangeMode.Name:
					return Resources.ArrangeByName;
				case PropertyArrangeMode.ValueSource:
					return Resources.ArrangeByValueSource;
				}
			}

			return value?.ToString ();
		}

		public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException ();
		}
	}
}