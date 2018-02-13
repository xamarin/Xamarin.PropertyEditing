using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace Xamarin.PropertyEditing.Windows
{
	[ValueConversion (typeof (Color), typeof (Color))]
	public class ColorToTransparentColorConverter : MarkupExtension, IValueConverter
	{
		public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
			=> value is Color color ? Color.FromArgb (0, color.R, color.G, color.B) : Color.FromArgb (0, 0, 0, 0);

		public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
			=> throw new NotImplementedException ();

		public override object ProvideValue (IServiceProvider serviceProvider) => this;
	}
}
