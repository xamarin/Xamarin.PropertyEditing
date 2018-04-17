using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.Windows
{
	[ValueConversion (typeof (CommonImageSource), typeof (ImageSource))]
	internal class CommonImageSourceToImageSourceConverter : MarkupExtension, IValueConverter
	{
		public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null) return null;
			if (value is CommonImageSource imageSource) return new BitmapImage(imageSource.UriSource);
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
