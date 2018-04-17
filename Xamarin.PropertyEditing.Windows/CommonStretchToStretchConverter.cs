using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.Windows
{
	[ValueConversion (typeof (CommonStretch), typeof (Stretch))]
	internal class CommonStretchToStretchConverter : MarkupExtension, IValueConverter
	{
		public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null) return null;
			if (value is CommonStretch stretch) return (Stretch)(stretch);
			return null;
		}

		public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException ();
		}

		public override object ProvideValue (IServiceProvider serviceProvider)
		{
			return this;
		}
	}
}
