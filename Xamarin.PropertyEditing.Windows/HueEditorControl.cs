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
				"Hue", typeof (CommonColor), typeof (HueEditorControl),
				new PropertyMetadata (new CommonColor(255, 0, 0), OnHueChanged));

		public CommonColor Hue {
			get => (CommonColor)GetValue (HueProperty);
			set => SetValue (HueProperty, value);
		}

		public static readonly DependencyProperty CursorPositionProperty =
			DependencyProperty.Register (
				"CursorPosition", typeof (double), typeof (HueEditorControl),
				new PropertyMetadata (0D));

		public double CursorPosition {
			get => (double)GetValue (CursorPositionProperty);
			set => SetValue (CursorPositionProperty, value);
		}

		public override void OnApplyTemplate ()
		{
			base.OnApplyTemplate ();

			hueChooser = (Rectangle)GetTemplateChild ("hueChooser");

			hueChooser.MouseLeftButtonDown += OnHuePicked;
			hueChooser.MouseMove += (s, e) => {
				if (IsEnabled && e.LeftButton == MouseButtonState.Pressed) {
					OnHuePicked(s, e);
				}
			};
			hueChooser.MouseLeftButtonUp += (s, e) => {
				RaiseEvent (new RoutedEventArgs (CommitCurrentColorEvent));
			};
		}

		protected override void OnRenderSizeChanged (SizeChangedInfo sizeInfo)
		{
			base.OnRenderSizeChanged (sizeInfo);
			CursorPosition = GetPositionFromHue (Hue) * ActualHeight;
		}

		void OnHuePicked (object s, MouseEventArgs e)
		{
			CursorPosition = e.GetPosition ((IInputElement)s).Y;
			Hue = GetHueFromPosition (CursorPosition / hueChooser.ActualHeight);
		}

		static void OnHueChanged (DependencyObject source, DependencyPropertyChangedEventArgs e)
		{
			var that = source as HueEditorControl;
			if (that == null) return;
			var newHue = ((CommonColor)e.NewValue);
			that.CursorPosition = GetPositionFromHue (newHue) * that.ActualHeight;
		}

		static readonly int[][] redRanges = new[] { new[] { 0, 1 }, new[] { 5, 6 } };
		static readonly int[][] greenRanges = new[] { new[] { 1, 3 } };
		static readonly int[][] blueRanges = new[] { new[] { 3, 5 } };

		/// <summary>
		/// Finds the hue from a position on the hue picker.
		/// 
		/// The hue dial is a gradient going through red, yellow, lime, cyan, blue, magenta, red.
		/// This means the following variations for red, green, and blue components:
		///      0 |   1 |   2 |   3 |   4 |   5 |   6
		/// -------|-----|-----|-----|-----|-----|-----
		/// R: 255 | 255 |   0 |   0 |   0 | 255 | 255
		/// G:   0 | 255 | 255 | 255 |   0 |   0 |   0
		/// B:   0 |   0 |   0 | 255 | 255 | 255 |   0
		/// </summary>
		/// <param name="position">The horizontal position on the hue picker, between 0 and 1</param>
		/// <returns>The hue</returns>
		static CommonColor GetHueFromPosition (double position)
		{
			var dialPosition = Math.Min(Math.Max(0, position * 6), 6);
			return new CommonColor (
				GetHueComponent (dialPosition, redRanges),
				GetHueComponent (dialPosition, greenRanges),
				GetHueComponent (dialPosition, blueRanges)
			);
		}

		/// <summary>
		/// Gets a color component between 0 and 255 based on a position
		/// between 0 and 6, and a set of ranges where the component is maxed out.
		/// The component varies from 0 to 255 over the position range 1 unit to the
		/// left of each interval, and from 255 to 0 over the position range 1 unit to
		/// the right of each interval
		/// </summary>
		/// <param name="position">The position selected, between 0 (included), and 6 (not included)</param>
		/// <param name="intervals">A set of intervals where the component is 255.</param>
		/// <returns>The value of the component.</returns>
		static byte GetHueComponent (double position, int[][] intervals)
		{
			if (position < 0 || position > 6)
				throw new ArgumentOutOfRangeException (nameof (position), "Position must be between 0 and 6.");
			foreach (var interval in intervals) {
				// Component is 255 inside the interval
				if (position >= interval[0] && position <= interval[1])
					return 255;
				// Component linearly grows from 0 to 255 one unit left of the interval
				if (position >= interval[0] - 1 && position < interval[0])
					return (byte)((position - interval[0] + 1) * 255);
				// Component linearly falls from 255 to 0 one unit right of the interval
				if (position > interval[1] && position <= interval[1] + 1)
					return (byte)(255 - (position - interval[1]) * 255);
			}
			// Otherwise, it's zero
			return 0;
		}

		/// <summary>
		/// Finds a position between 0 and 1 on the spectrum, given the hue.
		/// </summary>
		/// <param name="hue">The hue</param>
		/// <returns>A position on the spectrum between 0 and 1</returns>
		static double GetPositionFromHue(CommonColor hue)
		{
			if (hue.B == 0) {
				// We're between 0 and 2/6
				if (hue.R == 255) {
					// We're between 0 and 1/6, and green is on the rising phase
					return (double)hue.G / 255 / 6;
				}
				// We're between 1/6 and 2/6, and red is on the declining phase
				return ((double)(255 - hue.R) / 255 + 1) / 6;
			}
			else if (hue.R == 0) {
				// We're between 2/6 and 4/6
				if (hue.G == 255) {
					// We're between 2/6 and 3/6, and blue is on the rise
					return ((double)hue.B / 255 + 2) / 6;
				}
				// We're between 3/6 and 4/6, and green is declining
				return ((double)(255 - hue.G) / 255 + 3) / 6;
			}
			// We're between 4/6 and 6/6
			if (hue.B == 255) {
				// We're between 4/6 and 5/6, and red is on the rise
				return ((double)hue.R / 255 + 4) / 6;
			}
			// We're between 5/6 and 6/6, and blue is declining
			return ((double)(255 - hue.B) / 255 + 5) / 6;
		}
	}
}
