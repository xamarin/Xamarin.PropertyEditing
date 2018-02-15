using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.Windows
{
	[ValueConversion (typeof (CommonColor), typeof (Darkness))]
	internal class BrushToDarknessConverter : MarkupExtension, IValueConverter
	{
		public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is SolidColorBrush brush)) return Darkness.Unknown;
			return brush.Color.ToCommonColor ().Lightness > 0.667 ? Darkness.Light : Darkness.Dark;
		}

		public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
			=> throw new NotImplementedException ();

		public override object ProvideValue (IServiceProvider serviceProvider) => this;
	}

	internal enum Darkness
	{
		Dark, Light, Unknown
	}
}
