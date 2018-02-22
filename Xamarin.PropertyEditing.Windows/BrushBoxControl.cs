using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Xamarin.PropertyEditing.Windows
{
	internal class BrushBoxControl : Control
	{
		public static readonly DependencyProperty BrushProperty =
			DependencyProperty.Register (
				nameof (Brush), typeof (Brush), typeof (BrushBoxControl),
				new PropertyMetadata (new SolidColorBrush (Color.FromArgb (0, 0, 0, 0))));

		public Brush Brush {
			get => (Brush)GetValue (BrushProperty);
			set => SetValue (BrushProperty, value);
		}

		public static readonly DependencyProperty BrushVisibleProperty =
			DependencyProperty.Register (
				nameof (BrushVisible), typeof (Visibility), typeof (BrushBoxControl),
				new PropertyMetadata (Visibility.Hidden));

		public Visibility BrushVisible {
			get => (Visibility)GetValue (BrushVisibleProperty);
			set => SetValue (BrushVisibleProperty, value);
		}

		public static readonly DependencyProperty NoBrushVisibleProperty =
			DependencyProperty.Register (
				nameof (NoBrushVisible), typeof (Visibility), typeof (BrushBoxControl),
				new PropertyMetadata (Visibility.Visible));

		public Visibility NoBrushVisible {
			get => (Visibility)GetValue (NoBrushVisibleProperty);
			set => SetValue (NoBrushVisibleProperty, value);
		}

		public static readonly DependencyProperty LabelProperty =
			DependencyProperty.Register (
				nameof (Label), typeof (string), typeof (BrushBoxControl),
				new PropertyMetadata (null));

		public string Label
		{
			get => (string)GetValue (LabelProperty);
			set => SetValue (LabelProperty, value);
		}

		public static readonly DependencyProperty LightForegroundThresholdProperty =
			DependencyProperty.Register (
				nameof (LightForegroundThreshold), typeof (double), typeof (BrushBoxControl),
				new PropertyMetadata (0.667));

		public double LightForegroundThreshold
		{
			get => (double)GetValue (LightForegroundThresholdProperty);
			set => SetValue (LightForegroundThresholdProperty, value);
		}
	}
}
