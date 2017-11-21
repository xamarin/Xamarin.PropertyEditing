using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shapes;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.Windows
{
	internal class HueEditorControl : CurrentColorCommitterControlBase
	{
		public HueEditorControl ()
		{
			DefaultStyleKey = typeof (HueEditorControl);
		}

		Rectangle hueChooser;

		public static readonly DependencyProperty HueProperty =
			DependencyProperty.Register (
				nameof(HueColor), typeof (CommonColor), typeof (HueEditorControl),
				new PropertyMetadata (new CommonColor(255, 0, 0), OnHueChanged));

		public CommonColor HueColor {
			get => (CommonColor)GetValue (HueProperty);
			set => SetValue (HueProperty, value);
		}

		public static readonly DependencyProperty CursorPositionProperty =
			DependencyProperty.Register (
				nameof(CursorPosition), typeof (double), typeof (HueEditorControl),
				new PropertyMetadata (0D));

		public double CursorPosition {
			get => (double)GetValue (CursorPositionProperty);
			set => SetValue (CursorPositionProperty, value);
		}

		public override void OnApplyTemplate ()
		{
			base.OnApplyTemplate ();

			hueChooser = (Rectangle)GetTemplateChild ("hueChooser");

			hueChooser.MouseLeftButtonDown += (s, e) => {
				if (!hueChooser.IsMouseCaptured)
					hueChooser.CaptureMouse ();
			};
			hueChooser.MouseMove += (s, e) => {
				if (hueChooser.IsMouseCaptured && e.LeftButton == MouseButtonState.Pressed && hueChooser != null) {
					var cursorPosition = e.GetPosition ((IInputElement)s).Y;
					SetHueFromPosition (cursorPosition);
				}
			};
			hueChooser.MouseLeftButtonUp += (s, e) => {
				if (hueChooser.IsMouseCaptured) hueChooser.ReleaseMouseCapture ();
				var cursorPosition = e.GetPosition ((IInputElement)s).Y;
				SetHueFromPosition (cursorPosition);
				RaiseEvent (new RoutedEventArgs (CommitCurrentColorEvent));
			};
		}

		protected override void OnRenderSizeChanged (SizeChangedInfo sizeInfo)
		{
			base.OnRenderSizeChanged (sizeInfo);
			CursorPosition = CommonColor.GetHueFromHueColor (HueColor) * ActualHeight / 360;
		}

		void SetHueFromPosition (double cursorPosition)
		{
			if (cursorPosition < 0)
				cursorPosition = 0;
			if (cursorPosition >= hueChooser.ActualHeight)
				cursorPosition = hueChooser.ActualHeight - 1;
			HueColor = CommonColor.GetHueColorFromHue (360 * cursorPosition / hueChooser.ActualHeight);
		}

		static void OnHueChanged (DependencyObject source, DependencyPropertyChangedEventArgs e)
		{
			var that = source as HueEditorControl;
			if (that == null) return;
			var newHue = ((CommonColor)e.NewValue);
			that.CursorPosition = CommonColor.GetHueFromHueColor (newHue) * that.ActualHeight / 360;
		}
	}
}
