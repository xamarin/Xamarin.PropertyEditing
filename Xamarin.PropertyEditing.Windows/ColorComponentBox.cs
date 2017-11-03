using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Xamarin.PropertyEditing.Windows
{
	internal class ColorComponentBox : TextBox
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
