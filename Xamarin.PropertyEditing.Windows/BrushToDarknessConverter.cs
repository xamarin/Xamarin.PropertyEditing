using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.Windows
{
	internal class BrushToDarknessConverter : MarkupExtension, IMultiValueConverter
	{
		public object Convert (object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			if (values.Length == 0 || !(values[0] is SolidColorBrush brush)) return Darkness.Unknown;
			var threshold = (values.Length > 1 && values[1] is double doubleParameter) ? doubleParameter : 0.667;
			return brush.Color.ToCommonColor ().Lightness >= threshold ? Darkness.Light : Darkness.Dark;
		}

		public object[] ConvertBack (object value, Type[] targetTypes, object parameter, CultureInfo culture)
			=> throw new NotImplementedException ();

		public override object ProvideValue (IServiceProvider serviceProvider) => this;
	}

	internal enum Darkness
	{
		Dark, Light, Unknown
	}
}
