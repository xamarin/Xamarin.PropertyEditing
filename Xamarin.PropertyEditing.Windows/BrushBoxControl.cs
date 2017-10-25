using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Xamarin.PropertyEditing.Windows
{
	internal class BrushBoxControl : Control
	{
		public static readonly DependencyProperty BrushProperty =
			DependencyProperty.Register (
				nameof(Brush), typeof (Brush), typeof (ColorEditorControlBase),
				new PropertyMetadata (new SolidColorBrush (Color.FromArgb(0, 0, 0, 0))));

		public Brush Brush {
			get => (Brush)GetValue (BrushProperty);
			set => SetValue (BrushProperty, value);
		}
	}
}
