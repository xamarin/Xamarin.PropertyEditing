using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.Windows
{
	internal class ShadeEditorControl : Control
	{
		public ShadeEditorControl()
		{
			DefaultStyleKey = typeof (ShadeEditorControl);
		}

		Rectangle saturationLayer;
		Rectangle luminosityLayer;
		Canvas cursor;

		public static readonly DependencyProperty ShadeProperty =
			DependencyProperty.Register (
				"Shade", typeof (CommonColor), typeof (ShadeEditorControl),
				new PropertyMetadata (new CommonColor (0, 0, 0), OnShadeChanged));

		public CommonColor Shade {
			get => (CommonColor)GetValue (ShadeProperty);
			set => SetValue (ShadeProperty, value);
		}

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
				new PropertyMetadata (new CommonColor (255, 0, 0), HueChanged));

		public CommonColor Hue {
			get => (CommonColor)GetValue (HueProperty);
			set => SetValue (HueProperty, value);
		}

		public override void OnApplyTemplate ()
		{
			base.OnApplyTemplate ();

			saturationLayer = (Rectangle)GetTemplateChild ("saturationLayer");
			luminosityLayer = (Rectangle)GetTemplateChild ("luminosityLayer");
			cursor = (Canvas)GetTemplateChild ("cursor");

			luminosityLayer.MouseLeftButtonDown += OnCursorMoved;
			luminosityLayer.MouseMove += (s, e) => {
				if (e.LeftButton == MouseButtonState.Pressed) {
					OnCursorMoved (s, e);
				}
			};
		}

		void OnCursorMoved(object s, MouseEventArgs e)
		{
			CursorPosition = e.GetPosition ((IInputElement)s);
			SetValue (ShadeProperty, GetColorFromPosition (CursorPosition));
		}

		static void OnShadeChanged (DependencyObject source, DependencyPropertyChangedEventArgs e)
		{
			var that = source as ShadeEditorControl;
			if (that == null || that.cursor == null) return;
			that.CursorPosition = that.GetPositionFromColor ((CommonColor)e.NewValue);
		}

		static void HueChanged (DependencyObject source, DependencyPropertyChangedEventArgs e)
		{
			var that = source as ShadeEditorControl;
			if (that == null || that.saturationLayer == null) return;
			var saturationLayer = that.saturationLayer;
			var newBrush = (LinearGradientBrush)saturationLayer.Fill.Clone ();
			var gradientStops = newBrush.GradientStops;
			gradientStops.RemoveAt (1);
			var currentHue = that.Hue;
			gradientStops.Add (new GradientStop (Color.FromRgb (currentHue.R, currentHue.G, currentHue.B), 1));
			saturationLayer.Fill = newBrush;
			that.SetValue(ShadeProperty, that.GetColorFromPosition (that.CursorPosition));
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
			var brightness = 1 - position.Y / luminosityLayer.ActualHeight;

			return new CommonColor (
				(byte)((255 + (Hue.R - 255) * saturation) * brightness),
				(byte)((255 + (Hue.G - 255) * saturation) * brightness),
				(byte)((255 + (Hue.B - 255) * saturation) * brightness),
				255
			);
		}

		/// <summary>
		/// Finds a position on the shade chooser that corresponds to the passed-in
		/// color.
		/// </summary>
		/// <param name="color">The color for which we want the coordinates</param>
		/// <returns>The coordinates of the shade in the shade chooser</returns>
		Point GetPositionFromColor (CommonColor color)
		{
			var hue = color.ToHue ();
			var isRedMaxed = hue.R == 255;
			var isGreenMaxed = hue.G == 255;
			var alphaFactor = isRedMaxed ? (double)color.R / 255 : isGreenMaxed ? (double)color.G / 255 : (double)color.B / 255;
			var isRedMin = hue.R == 0;
			var isGreenMin = hue.G == 0;

			var xrate =
				isRedMin ? (color.R / alphaFactor - 255) / (hue.R - 255)
				: isGreenMin ? (color.G / alphaFactor - 255) / (hue.G - 255)
				: (color.B / alphaFactor - 255) / (hue.B - 255);

			return new Point (
				xrate * saturationLayer.ActualWidth + saturationLayer.Margin.Left ,
				(1 - alphaFactor) * luminosityLayer.ActualHeight + luminosityLayer.Margin.Top);
		}
	}
}
