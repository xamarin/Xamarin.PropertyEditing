using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.Windows
{
	internal class ColorComponentsEditorControl : Control
    {
		public ColorComponentsEditorControl()
		{
			DefaultStyleKey = typeof (ColorComponentsEditorControl);
		}

		public static readonly DependencyProperty ColorProperty =
			DependencyProperty.Register (
				"Color", typeof (CommonColor), typeof (ColorComponentsEditorControl),
				new PropertyMetadata (new CommonColor (0, 0, 0)));

		public CommonColor Color {
			get => (CommonColor)GetValue (ColorProperty);
			set => SetValue (ColorProperty, value);
		}
    }

	[ValueConversion(typeof(CommonColor), typeof(string))]
	internal class HexColorConverter : MarkupExtension, IValueConverter
	{
		public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is CommonColor)) return "#FF000000";

			var color = (CommonColor)value;
			return $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
		}

		public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
		{
			var stringValue = value as string;
			if (string.IsNullOrWhiteSpace (stringValue)) return new CommonColor (0, 0, 0);
			var color = (Color)ColorConverter.ConvertFromString (stringValue);
			return new CommonColor (color.R, color.G, color.B, color.A);
		}

		public override object ProvideValue (IServiceProvider serviceProvider)
		{
			return this;
		}
	}
}
