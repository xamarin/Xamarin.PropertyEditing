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
				new PropertyMetadata (new CommonColor (0, 0, 0), OnColorChanged));

		public CommonColor Color {
			get => (CommonColor)GetValue (ColorProperty);
			set => SetValue (ColorProperty, value);
		}

		public static readonly DependencyProperty RedProperty =
			DependencyProperty.Register (
				"R", typeof (byte), typeof (ColorComponentsEditorControl),
				new PropertyMetadata ((byte)0, OnComponentChanged));

		public byte R {
			get => (byte)GetValue (RedProperty);
			set => SetValue (RedProperty, value);
		}

		public static readonly DependencyProperty GreenProperty =
			DependencyProperty.Register (
				"G", typeof (byte), typeof (ColorComponentsEditorControl),
				new PropertyMetadata ((byte)0, OnComponentChanged));

		public byte G {
			get => (byte)GetValue (GreenProperty);
			set => SetValue (GreenProperty, value);
		}

		public static readonly DependencyProperty BlueProperty =
			DependencyProperty.Register (
				"B", typeof (byte), typeof (ColorComponentsEditorControl),
				new PropertyMetadata ((byte)0, OnComponentChanged));

		public byte B {
			get => (byte)GetValue (BlueProperty);
			set => SetValue (BlueProperty, value);
		}

		public static readonly DependencyProperty AlphaProperty =
			DependencyProperty.Register (
				"A", typeof (byte), typeof (ColorComponentsEditorControl),
				new PropertyMetadata ((byte)0, OnComponentChanged));

		public byte A {
			get => (byte)GetValue (AlphaProperty);
			set => SetValue (AlphaProperty, value);
		}

		private static void OnComponentChanged (DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var control = (ColorComponentsEditorControl)d;
			var newColor = new CommonColor (control.R, control.G, control.B, control.A);
			if (!newColor.Equals(control.Color)) {
				control.Color = newColor;
			}
		}

		private static void OnColorChanged (DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var control = (ColorComponentsEditorControl)d;
			var newColor = (CommonColor)e.NewValue;
			if (control.R != newColor.R) control.R = newColor.R;
			if (control.G != newColor.G) control.G = newColor.G;
			if (control.B != newColor.B) control.B = newColor.B;
			if (control.A != newColor.A) control.A = newColor.A;
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
