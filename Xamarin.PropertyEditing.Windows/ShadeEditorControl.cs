using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.Windows
{
	internal class ShadeEditorControl : ColorEditorControlBase
	{
		public ShadeEditorControl()
		{
			DefaultStyleKey = typeof (ShadeEditorControl);
		}

		Rectangle saturationLayer;
		Rectangle luminosityLayer;

		public static readonly DependencyProperty CursorPositionProperty =
			DependencyProperty.Register (
				"CursorPosition", typeof (Point), typeof (ShadeEditorControl),
				new PropertyMetadata (new Point (0, 0)));

		public Point CursorPosition {
			get => (Point)GetValue (CursorPositionProperty);
			set => SetValue (CursorPositionProperty, value);
		}

		public static readonly DependencyProperty HueProperty =
			DependencyProperty.Register (
				"Hue", typeof (CommonColor), typeof (ShadeEditorControl),
				new PropertyMetadata (new CommonColor (255, 0, 0), OnHuePropertyChanged));

		public CommonColor Hue {
			get => (CommonColor)GetValue (HueProperty);
			set => SetValue (HueProperty, value);
		}

		public override void OnApplyTemplate ()
		{
			base.OnApplyTemplate ();

			saturationLayer = (Rectangle)GetTemplateChild ("saturationLayer");
			luminosityLayer = (Rectangle)GetTemplateChild ("luminosityLayer");

			OnHueChanged (Hue);

			luminosityLayer.MouseLeftButtonDown += (s, e) => {
				if (!luminosityLayer.IsMouseCaptured)
					luminosityLayer.CaptureMouse ();
			};
			luminosityLayer.MouseMove += (s, e) => {
				if (luminosityLayer.IsMouseCaptured && e.LeftButton == MouseButtonState.Pressed && saturationLayer != null) {
					var cursorPosition = e.GetPosition ((IInputElement)s);
					SetColorFromMousePosition (cursorPosition);
				}
			};
			luminosityLayer.MouseLeftButtonUp += (s, e) => {
				if (luminosityLayer.IsMouseCaptured) luminosityLayer.ReleaseMouseCapture ();
				var cursorPosition = e.GetPosition ((IInputElement)s);
				SetColorFromMousePosition (cursorPosition);
				RaiseEvent (new RoutedEventArgs (CommitShadeEvent));
			};
		}

		void SetColorFromMousePosition(Point cursorPosition)
		{
			if (cursorPosition.X < 0)
				cursorPosition.X = 0;
			if (cursorPosition.X > luminosityLayer.ActualWidth)
				cursorPosition.X = luminosityLayer.ActualWidth;
			if (cursorPosition.Y < 0)
				cursorPosition.Y = 0;
			if (cursorPosition.Y > luminosityLayer.ActualHeight)
				cursorPosition.Y = luminosityLayer.ActualHeight;
			CursorPosition = cursorPosition;
			var newColor = GetColorFromPosition (cursorPosition);
			Color = new CommonColor (newColor.R, newColor.G, newColor.B, Color.A);
		}

		protected override void OnRenderSizeChanged (SizeChangedInfo sizeInfo)
		{
			base.OnRenderSizeChanged (sizeInfo);

			CursorPosition = GetPositionFromColor (Color);
		}

		protected override void OnColorChanged (CommonColor oldColor, CommonColor newColor)
		{
			base.OnColorChanged (oldColor, newColor);

			if (luminosityLayer == null || !luminosityLayer.IsMouseCaptured)
				CursorPosition = GetPositionFromColor (newColor);
		}

		static void OnHuePropertyChanged (DependencyObject source, DependencyPropertyChangedEventArgs e)
		{
			var shadeEditor = source as ShadeEditorControl;
			if (shadeEditor != null) {
				shadeEditor.OnHueChanged ((CommonColor)e.NewValue);
			}
		}

		private void OnHueChanged (CommonColor newHue)
		{
			if (saturationLayer == null) return;
			var newBrush = (LinearGradientBrush)saturationLayer.Fill.Clone ();
			var gradientStops = newBrush.GradientStops;
			gradientStops.RemoveAt (1);
			gradientStops.Add (new GradientStop (System.Windows.Media.Color.FromRgb (newHue.R, newHue.G, newHue.B), 1));
			saturationLayer.Fill = newBrush;
		}

		/// <summary>
		/// Maps coordinates within the shade chooser gradients into colors.
		/// The gradients have the current hue on the top-right corner,
		/// black along the whole bottom border, and white of the top-left.
		///
		/// For example, with a hue of 128,255,0:
		/// 
		/// 255,255,255---192,255,128---128,255,000
		///      |             |             |
		/// 128,128,128---096,128,064---064,128,000
		///      |             |             |
		/// 000,000,000---000,000,000---000,000,000
		/// 
		/// The horizontal axis corresponds roughly to saturation, and the
		/// vertical axis to brightness.
		/// </summary>
		/// <param name="position">The position for which to infer the color</param>
		/// <returns>The shade</returns>
		CommonColor GetColorFromPosition (Point position)
		{
			var saturation = position.X / saturationLayer.ActualWidth;
			var luminosity = 1 - position.Y / luminosityLayer.ActualHeight;

			return CommonColor.From (Hue, luminosity, saturation);
		}

		/// <summary>
		/// Finds a position on the shade chooser that corresponds to the passed-in
		/// color.
		/// </summary>
		/// <param name="color">The color for which we want the coordinates</param>
		/// <returns>The coordinates of the shade in the shade chooser</returns>
		Point GetPositionFromColor (CommonColor color)
		{
			var luminosity = color.Luminosity;
			var saturation = color.Saturation;

			if (saturationLayer == null || luminosityLayer == null) return new Point (0, 0);
			return new Point (
				saturation * saturationLayer.ActualWidth + saturationLayer.Margin.Left ,
				(1 - luminosity) * luminosityLayer.ActualHeight + luminosityLayer.Margin.Top);
		}
	}
}
