using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Xamarin.PropertyEditing.Windows
{
	internal class MultiplierConverter
		: IValueConverter
	{
		public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
		{
			double multiplier = (double)System.Convert.ChangeType (parameter, typeof(double));
			return Math.Round (((double) value) * multiplier);
		}

		public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException ();
		}
	}
}
