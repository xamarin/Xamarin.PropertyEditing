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
	[ValueConversion (typeof (CommonBrush), typeof (Brush))]
	internal class CommonBrushToBrushConverter : MarkupExtension, IValueConverter
	{
		public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null) return null;
			if (value is CommonSolidBrush solidBrush) return new SolidColorBrush {
				Color = solidBrush.Color.ToColor (),
				Opacity = solidBrush.Opacity
			};

			if (value is CommonImageBrush commonImageBrush) {
				var imageBrush = new ImageBrush {
					AlignmentX = (AlignmentX)commonImageBrush.AlignmentX,
					AlignmentY = (AlignmentY)commonImageBrush.AlignmentY,
					Opacity = commonImageBrush.Opacity,
					Stretch = (Stretch)commonImageBrush.Stretch,
					TileMode = (TileMode)commonImageBrush.TileMode,
					Viewbox = commonImageBrush.ViewBox.ToRect (),
					ViewboxUnits = (BrushMappingMode)commonImageBrush.ViewBoxUnits,
					Viewport = commonImageBrush.ViewPort.ToRect (),
					ViewportUnits = (BrushMappingMode)commonImageBrush.ViewPortUnits
				};
				if (commonImageBrush.ImageSource != null && commonImageBrush.ImageSource.UriSource != null) {
					var image = new BitmapImage (commonImageBrush.ImageSource.UriSource);
					imageBrush.ImageSource = image;
				}
				return imageBrush;
			}
			// TODO: revisit this with more details when we build the editors for those brushes
			if (value is CommonLinearGradientBrush linearGradientBrush) {
				return new LinearGradientBrush (
					new GradientStopCollection (linearGradientBrush.GradientStops.Select (stop => new GradientStop (stop.Color.ToColor (), stop.Offset))),
					linearGradientBrush.StartPoint.ToPoint (), linearGradientBrush.EndPoint.ToPoint ());
			}
			if (value is CommonRadialGradientBrush radialGradientBrush) {
				return new RadialGradientBrush (
					new GradientStopCollection (radialGradientBrush.GradientStops.Select (stop => new GradientStop (stop.Color.ToColor (), stop.Offset))));
			}
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
