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
				new PropertyMetadata (new CommonColor (255, 0, 0), OnHueChanged));

		public CommonColor Hue {
			get => (CommonColor)GetValue (HueProperty);
			set => SetValue (HueProperty, value);
		}

		public override void OnApplyTemplate ()
		{
			base.OnApplyTemplate ();

			saturationLayer = (Rectangle)GetTemplateChild ("saturationLayer");
			luminosityLayer = (Rectangle)GetTemplateChild ("luminosityLayer");

			luminosityLayer.MouseLeftButtonDown += OnCursorMoved;
			luminosityLayer.MouseMove += (s, e) => {
				if (e.LeftButton == MouseButtonState.Pressed) {
					OnCursorMoved (s, e);
				}
			};
			luminosityLayer.MouseLeftButtonUp += (s, e) => {
				RaiseEvent (new RoutedEventArgs (CommitCurrentColorEvent));
			};
		}

		protected override void OnRenderSizeChanged (SizeChangedInfo sizeInfo)
		{
			base.OnRenderSizeChanged (sizeInfo);

			CursorPosition = GetPositionFromColor (Color);
			Hue = Color.Hue;
		}

		void OnCursorMoved(object source, MouseEventArgs e)
		{
			// TODO: find a way to prevent the hue from changing when picking a new shade because of lack of resolution near greys.
			CursorPosition = e.GetPosition ((IInputElement)source);
			if (saturationLayer == null) return;
			var newColor = GetColorFromPosition (CursorPosition);
			Color = new CommonColor (newColor.R, newColor.G, newColor.B, Color.A);
		}

		protected override void OnColorChanged (CommonColor oldColor, CommonColor newColor)
		{
			base.OnColorChanged (oldColor, newColor);

			CursorPosition = GetPositionFromColor (newColor);
			var oldHue = oldColor.Hue;
			var newHue = newColor.Hue;
			if (!newHue.Equals(oldHue) && !newColor.IsGrey) {
				OnHueChanged (this, new DependencyPropertyChangedEventArgs (HueProperty, oldHue, newHue));
			}
		}

		static void OnHueChanged (DependencyObject source, DependencyPropertyChangedEventArgs e)
		{
			var that = source as ShadeEditorControl;
			if (that == null || that.saturationLayer == null) return;
			var saturationLayer = that.saturationLayer;
			var newBrush = (LinearGradientBrush)saturationLayer.Fill.Clone ();
			var gradientStops = newBrush.GradientStops;
			gradientStops.RemoveAt (1);
			var newHue = (CommonColor)e.NewValue;
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

			return CommonColor.From (Color.Hue, luminosity, saturation);
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
