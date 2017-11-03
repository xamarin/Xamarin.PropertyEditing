using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Xamarin.PropertyEditing.Windows
{
	internal class ColorComponentBox : Control
	{
		static ColorComponentBox ()
		{
			DefaultStyleKeyProperty.OverrideMetadata (
				typeof (ColorComponentBox),
				new FrameworkPropertyMetadata (typeof (ColorComponentBox)));
		}

		public static readonly DependencyProperty GradientBrushProperty =
			DependencyProperty.Register (
				nameof (GradientBrush), typeof (Brush), typeof (ColorComponentBox),
				new PropertyMetadata (new SolidColorBrush ()));

		public Brush GradientBrush {
			get => (Brush)GetValue (GradientBrushProperty);
			set => SetValue (GradientBrushProperty, value);
		}

		public static readonly DependencyProperty UnitProperty =
			DependencyProperty.Register (
				nameof (Unit), typeof (string), typeof (ColorComponentBox),
				new PropertyMetadata (""));

		public string Unit {
			get => (string)GetValue (UnitProperty);
			set => SetValue (UnitProperty, value);
		}

		public static readonly DependencyProperty ValueProperty =
			DependencyProperty.Register (
				nameof (Value), typeof (double), typeof (ColorComponentBox),
				new PropertyMetadata (0d));

		public double Value {
			get => (double)GetValue (ValueProperty);
			set => SetValue (ValueProperty, value);
		}

		TextBox innerTextBox;

		public override void OnApplyTemplate ()
		{
			base.OnApplyTemplate ();

			innerTextBox = GetTemplateChild ("innerTextBox") as TextBox;
			if (innerTextBox != null) {
				innerTextBox.GotKeyboardFocus += (s, e) => {
					innerTextBox.SelectAll ();
				};
				innerTextBox.PreviewMouseLeftButtonDown += (s, e) => {
					innerTextBox.Focus ();
					e.Handled = true;
				};
			}
		}
	}
}
