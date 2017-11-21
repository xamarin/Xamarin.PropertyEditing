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
			var color = (CommonColor)value;
			return ConvertImplementation (
				Color.FromArgb (color.A, color.R, color.G, 0),
				Color.FromArgb (color.A, color.R, color.G, 255));
		}
	}

	internal class ColorComponentToCyanBrushConverter : ColorComponentToBrushConverterBase
	{
		public override object Convert (object value, Type targetType, object parameter, CultureInfo culture)
		{
			var color = (CommonColor)value;
			var colorStart = CommonColor.FromCMYK (0, color.M, color.Y, color.K, color.A).ToColor ();
			var colorEnd = CommonColor.FromCMYK (1, color.M, color.Y, color.K, color.A).ToColor ();

			return ConvertImplementation (colorStart, colorEnd);
		}
	}

	internal class ColorComponentToMagentaBrushConverter : ColorComponentToBrushConverterBase
	{
		public override object Convert (object value, Type targetType, object parameter, CultureInfo culture)
		{
			var color = (CommonColor)value;
			var colorStart = CommonColor.FromCMYK (color.C, 0, color.Y, color.K, color.A).ToColor ();
			var colorEnd = CommonColor.FromCMYK (color.C, 1, color.Y, color.K, color.A).ToColor ();

			return ConvertImplementation (colorStart, colorEnd);
		}
	}

	internal class ColorComponentToYellowBrushConverter : ColorComponentToBrushConverterBase
	{
		public override object Convert (object value, Type targetType, object parameter, CultureInfo culture)
		{
			var color = (CommonColor)value;
			var colorStart = CommonColor.FromCMYK (color.C, color.M, 0, color.K, color.A).ToColor ();
			var colorEnd = CommonColor.FromCMYK (color.C, color.M, 1, color.K, color.A).ToColor ();

			return ConvertImplementation (colorStart, colorEnd);
		}
	}

	internal class ColorComponentToBlackBrushConverter : ColorComponentToBrushConverterBase
	{
		public override object Convert (object value, Type targetType, object parameter, CultureInfo culture)
		{
			var color = (CommonColor)value;
			var colorStart = CommonColor.FromCMYK (color.C, color.M, color.Y, 0, color.A).ToColor ();
			var colorEnd = CommonColor.FromCMYK (color.C, color.M, color.Y, 1, color.A).ToColor ();

			return ConvertImplementation (colorStart, colorEnd);
		}
	}

	internal class ColorComponentToSaturationBrushConverter : ColorComponentToBrushConverterBase
	{
		public override object Convert (object value, Type targetType, object parameter, CultureInfo culture)
		{
			var color = (CommonColor)value;
			var hue = color.Hue;
			var lightness = color.Lightness;
			var colorStart = CommonColor.FromHLS (hue, lightness, 0).ToColor ();
			var colorEnd = CommonColor.FromHLS (hue, lightness, 1).ToColor ();

			return ConvertImplementation (colorStart, colorEnd);
		}
	}

	internal class ColorComponentToLightnessBrushConverter : ColorComponentToBrushConverterBase
	{
		public override object Convert (object value, Type targetType, object parameter, CultureInfo culture)
		{
			var color = (CommonColor)value;
			var hue = color.Hue;
			var saturation = color.Saturation;
			var colorStart = CommonColor.FromHLS(hue, 0, saturation).ToColor ();
			var colorEnd = CommonColor.FromHLS(hue, 1, saturation).ToColor ();

			return new LinearGradientBrush(new GradientStopCollection {
				new GradientStop(colorStart, 0),
				new GradientStop(color.ToColor(), color.Lightness),
				new GradientStop(colorEnd, 1)
			}, 0);
		}
	}

	internal class ColorComponentToBrightnessBrushConverter : ColorComponentToBrushConverterBase
	{
		public override object Convert (object value, Type targetType, object parameter, CultureInfo culture)
		{
			var color = (CommonColor)value;
			var hue = color.Hue;
			var saturation = color.Saturation;
			var colorStart = CommonColor.FromHSB (hue, saturation, 0).ToColor ();
			var colorEnd = CommonColor.FromHSB (hue, saturation, 1).ToColor ();

			return ConvertImplementation (colorStart, colorEnd);
		}
	}
}
