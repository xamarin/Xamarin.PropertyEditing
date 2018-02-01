using System;
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
		Rectangle brightnessLayer;

		public static readonly DependencyProperty CursorPositionProperty =
			DependencyProperty.Register (
				nameof(CursorPosition), typeof (Point), typeof (ShadeEditorControl),
				new PropertyMetadata (new Point (0, 0)));

		public Point CursorPosition {
			get => (Point)GetValue (CursorPositionProperty);
			set => SetValue (CursorPositionProperty, value);
		}

		public static readonly DependencyProperty HueProperty =
			DependencyProperty.Register (
				nameof(HueColor), typeof (CommonColor), typeof (ShadeEditorControl),
				new PropertyMetadata (new CommonColor (255, 0, 0), OnHuePropertyChanged));

		public CommonColor HueColor {
			get => (CommonColor)GetValue (HueProperty);
			set => SetValue (HueProperty, value);
		}

		public override void OnApplyTemplate ()
		{
			base.OnApplyTemplate ();

			this.saturationLayer = GetTemplateChild ("saturationLayer") as Rectangle;
			this.brightnessLayer = GetTemplateChild ("brightnessLayer") as Rectangle;

			OnHueChanged (HueColor);

			if (this.saturationLayer == null)
				throw new InvalidOperationException ($"{nameof (ShadeEditorControl)} is missing a child Rectangle named \"saturationLayer\"");
			if (this.brightnessLayer == null)
				throw new InvalidOperationException ($"{nameof (ShadeEditorControl)} is missing a child Rectangle named \"brightnessLayer\"");

			this.brightnessLayer.MouseLeftButtonDown += (s, e) => {
				if (!this.brightnessLayer.IsMouseCaptured)
					this.brightnessLayer.CaptureMouse ();
			};
			this.brightnessLayer.MouseMove += (s, e) => {
				if (this.brightnessLayer.IsMouseCaptured && e.LeftButton == MouseButtonState.Pressed && this.saturationLayer != null) {
					Point cursorPosition = e.GetPosition ((IInputElement)s);
					SetColorFromMousePosition (cursorPosition);
				}
			};
			this.brightnessLayer.MouseLeftButtonUp += (s, e) => {
				if (this.brightnessLayer.IsMouseCaptured) this.brightnessLayer.ReleaseMouseCapture ();
				Point cursorPosition = e.GetPosition ((IInputElement)s);
				SetColorFromMousePosition (cursorPosition);
				RaiseEvent (new RoutedEventArgs (CommitShadeEvent));
			};
		}

		void SetColorFromMousePosition(Point cursorPosition)
		{
			if (cursorPosition.X < 0)
				cursorPosition.X = 0;
			if (cursorPosition.X > this.brightnessLayer.ActualWidth)
				cursorPosition.X = this.brightnessLayer.ActualWidth;
			if (cursorPosition.Y < 0)
				cursorPosition.Y = 0;
			if (cursorPosition.Y > this.brightnessLayer.ActualHeight)
				cursorPosition.Y = this.brightnessLayer.ActualHeight;

			CursorPosition = cursorPosition;
			CommonColor newColor = GetColorFromPosition (cursorPosition);
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

			if (this.brightnessLayer == null || !this.brightnessLayer.IsMouseCaptured)
				CursorPosition = GetPositionFromColor (newColor);
		}

		static void OnHuePropertyChanged (DependencyObject source, DependencyPropertyChangedEventArgs e)
		{
			if (source is ShadeEditorControl shadeEditor) {
				shadeEditor.OnHueChanged ((CommonColor)e.NewValue);
			}
		}

		private void OnHueChanged (CommonColor newHue)
		{
			// OnHueChanged may be called before the template is applied, so the layer may not yet be available.
			if (this.saturationLayer == null) return;
			var newBrush = (LinearGradientBrush)this.saturationLayer.Fill.Clone ();
			GradientStopCollection gradientStops = newBrush.GradientStops;
			gradientStops.RemoveAt (1);
			gradientStops.Add (new GradientStop (System.Windows.Media.Color.FromRgb (newHue.R, newHue.G, newHue.B), 1));
			this.saturationLayer.Fill = newBrush;
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
			var saturation = position.X / this.saturationLayer.ActualWidth;
			var brightness = 1 - position.Y / this.brightnessLayer.ActualHeight;

			return CommonColor.FromHSB (HueColor.Hue, saturation, brightness);
		}

		/// <summary>
		/// Finds a position on the shade chooser that corresponds to the passed-in
		/// color.
		/// </summary>
		/// <param name="color">The color for which we want the coordinates</param>
		/// <returns>The coordinates of the shade in the shade chooser</returns>
		Point GetPositionFromColor (CommonColor color)
		{
			var brightness = color.Brightness;
			var saturation = color.Saturation;

			// OnHueChanged may be called before the template is applied, so the layer may not yet be available.
			if (this.saturationLayer == null || this.brightnessLayer == null) return new Point (0, 0);

			return new Point (
				saturation * this.saturationLayer.ActualWidth + this.saturationLayer.Margin.Left ,
				(1 - brightness) * this.brightnessLayer.ActualHeight + this.brightnessLayer.Margin.Top);
		}
	}
}
