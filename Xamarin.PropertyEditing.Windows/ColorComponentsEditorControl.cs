using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.Windows
{
	internal class ColorComponentsEditorControl : ColorEditorControlBase
	{
		public ColorComponentsEditorControl()
		{
			DefaultStyleKey = typeof (ColorComponentsEditorControl);
		}

		public static readonly DependencyProperty RedProperty =
			DependencyProperty.Register (
				nameof(R), typeof (byte), typeof (ColorComponentsEditorControl),
				new PropertyMetadata ((byte)0));

		public byte R {
			get => (byte)GetValue (RedProperty);
			set => SetValue (RedProperty, value);
		}

		public static readonly DependencyProperty GreenProperty =
			DependencyProperty.Register (
				nameof(G), typeof (byte), typeof (ColorComponentsEditorControl),
				new PropertyMetadata ((byte)0));

		public byte G {
			get => (byte)GetValue (GreenProperty);
			set => SetValue (GreenProperty, value);
		}

		public static readonly DependencyProperty BlueProperty =
			DependencyProperty.Register (
				nameof(B), typeof (byte), typeof (ColorComponentsEditorControl),
				new PropertyMetadata ((byte)0));

		public byte B {
			get => (byte)GetValue (BlueProperty);
			set => SetValue (BlueProperty, value);
		}

		public static readonly DependencyProperty AlphaProperty =
			DependencyProperty.Register (
				nameof(A), typeof (byte), typeof (ColorComponentsEditorControl),
				new PropertyMetadata ((byte)0));

		public byte A {
			get => (byte)GetValue (AlphaProperty);
			set => SetValue (AlphaProperty, value);
		}

		public override void OnApplyTemplate ()
		{
			base.OnApplyTemplate ();

			foreach(var focusable in GetFocusableDescendants(this)) {
				focusable.LostFocus += OnBlur;
			}
		}

		IEnumerable<UIElement> GetFocusableDescendants(UIElement parent)
		{
			var childCount = VisualTreeHelper.GetChildrenCount (parent);
			for (var i = 0; i < childCount; i++) {
				var child = VisualTreeHelper.GetChild (parent, i) as UIElement;
				if (child != null) {
					if (child.Focusable) yield return child;
					var grandChildren = GetFocusableDescendants (child);
					foreach (var grandChild in grandChildren) yield return grandChild;
				}
			}
		}

		void OnBlur (object sender, RoutedEventArgs e)
		{
			var newColor = new CommonColor (R, G, B, A);
			if (!newColor.Equals (Color)) {
				Color = newColor;
			}

			RaiseEvent (new RoutedEventArgs (CommitCurrentColorEvent));
		}

		protected override void OnColorChanged (CommonColor oldColor, CommonColor newColor)
		{
			base.OnColorChanged (oldColor, newColor);

			if (R != newColor.R) R = newColor.R;
			if (G != newColor.G) G = newColor.G;
			if (B != newColor.B) B = newColor.B;
			if (A != newColor.A) A = newColor.A;
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
