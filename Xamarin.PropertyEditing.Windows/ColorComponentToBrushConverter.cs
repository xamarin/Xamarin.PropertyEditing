using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.Windows
{
	[ValueConversion (typeof (CommonColor), typeof (Brush))]
	internal abstract class ColorComponentToBrushConverterBase : MarkupExtension, IValueConverter
	{
		public abstract object Convert (object value, Type targetType, object parameter, CultureInfo culture);

		protected object ConvertImplementation (Color leftColor, Color rightColor)
			=> new LinearGradientBrush (leftColor, rightColor, 0);

		public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException ();
		}

		public override object ProvideValue (IServiceProvider serviceProvider)
		{
			return this;
		}
	}

	internal class ColorComponentToAlphaBrushConverter : ColorComponentToBrushConverterBase
	{
		public override object Convert (object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is CommonColor))
				return new SolidColorBrush (Color.FromArgb (0, 0, 0, 0));
			var color = (CommonColor)value;
			return ConvertImplementation (
				Color.FromArgb (0, color.R, color.G, color.B),
				Color.FromArgb (255, color.R, color.G, color.B));
		}
	}

	internal class ColorComponentToRedBrushConverter : ColorComponentToBrushConverterBase
	{
		public override object Convert (object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is CommonColor))
				return new SolidColorBrush (Color.FromArgb (0, 0, 0, 0));
			var color = (CommonColor)value;
			return ConvertImplementation (
				Color.FromArgb (color.A, 0, color.G, color.B),
				Color.FromArgb (color.A, 255, color.G, color.B));
		}
	}

	internal class ColorComponentToGreenBrushConverter : ColorComponentToBrushConverterBase
	{
		public override object Convert (object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is CommonColor))
				return new SolidColorBrush (Color.FromArgb (0, 0, 0, 0));
			var color = (CommonColor)value;
			return ConvertImplementation (
				Color.FromArgb (color.A, color.R, 0, color.B),
				Color.FromArgb (color.A, color.R, 255, color.B));
		}
	}

	internal class ColorComponentToBlueBrushConverter : ColorComponentToBrushConverterBase
	{
		public override object Convert (object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is CommonColor))
				return new SolidColorBrush (Color.FromArgb (0, 0, 0, 0));
			var color = (CommonColor)value;
			return ConvertImplementation (
				Color.FromArgb (color.A, color.R, color.G, 0),
				Color.FromArgb (color.A, color.R, color.G, 255));
		}
	}
}
