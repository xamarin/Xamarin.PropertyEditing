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

			this.hueChooser = (Rectangle)GetTemplateChild ("hueChooser");

			this.hueChooser.MouseLeftButtonDown += (s, e) => {
				if (!this.hueChooser.IsMouseCaptured)
					this.hueChooser.CaptureMouse ();
			};
			this.hueChooser.MouseMove += (s, e) => {
				if (this.hueChooser.IsMouseCaptured && e.LeftButton == MouseButtonState.Pressed && this.hueChooser != null) {
					var cursorPosition = e.GetPosition ((IInputElement)s).Y;
					SetHueFromPosition (cursorPosition);
				}
			};
			this.hueChooser.MouseLeftButtonUp += (s, e) => {
				if (this.hueChooser.IsMouseCaptured) this.hueChooser.ReleaseMouseCapture ();
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
			if (cursorPosition >= this.hueChooser.ActualHeight)
				cursorPosition = this.hueChooser.ActualHeight - 1;
			HueColor = CommonColor.GetHueColorFromHue (360 * cursorPosition / this.hueChooser.ActualHeight);
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
