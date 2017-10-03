using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Xamarin.PropertyEditing.Drawing;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Windows
{
	internal class SolidBrushEditorControl
		: PropertyEditorControl
	{
		public SolidBrushEditorControl ()
		{
			DefaultStyleKey = typeof (SolidBrushEditorControl);
		}

		TextBox redEntry;
		TextBox greenEntry;
		TextBox blueEntry;
		TextBox alphaEntry;
		Rectangle topShadeChooser;
		Rectangle bottomShadeChooser;
		Rectangle hueChooser;
		Canvas cursor;
		double cursorWidth;
		double cursorHeight;

		SolidBrushPropertyViewModel ViewModel => DataContext as SolidBrushPropertyViewModel;

		CommonColor CurrentHue = new CommonColor (255, 0, 0);
		Point CurrentCursorPosition;

		public override void OnApplyTemplate ()
		{
			base.OnApplyTemplate ();

			redEntry = (TextBox)GetTemplateChild ("redEntry");
			greenEntry = (TextBox)GetTemplateChild ("greenEntry");
			blueEntry = (TextBox)GetTemplateChild ("blueEntry");
			alphaEntry = (TextBox)GetTemplateChild ("alphaEntry");
			hueChooser = (Rectangle)GetTemplateChild ("hueChooser");
			topShadeChooser = (Rectangle)GetTemplateChild ("topShadeChooser");
			bottomShadeChooser = (Rectangle)GetTemplateChild ("bottomShadeChooser");
			cursor = (Canvas)GetTemplateChild ("cursor");
			cursorWidth = (cursor.Children.Cast<Shape> ()).Max (c => c.Width);
			cursorHeight = (cursor.Children.Cast<Shape> ()).Max (c => c.Height);

			if (ViewModel.Property.CanWrite) {
				// Handle changes on ARGB text boxes
				redEntry.PreviewTextInput += ConstrainToDigits;
				redEntry.LostFocus += (s, e) => UpdateFromTextBoxes ();
				greenEntry.PreviewTextInput += ConstrainToDigits;
				greenEntry.LostFocus += (s, e) => UpdateFromTextBoxes ();
				blueEntry.PreviewTextInput += ConstrainToDigits;
				blueEntry.LostFocus += (s, e) => UpdateFromTextBoxes ();
				alphaEntry.PreviewTextInput += ConstrainToDigits;
				alphaEntry.LostFocus += (s, e) => UpdateFromTextBoxes ();

				// Handle interaction with the hue chooser
				hueChooser.MouseLeftButtonDown += OnHueChanged;
				hueChooser.MouseMove += (s, e) => {
					if (e.LeftButton == MouseButtonState.Pressed) {
						OnHueChanged(s, e);
					}
				};

				// Handle interaction with the top shade chooser
				topShadeChooser.MouseLeftButtonDown += OnShadeChanged;
				topShadeChooser.MouseMove += (s, e) => {
					if (e.LeftButton == MouseButtonState.Pressed) {
						OnShadeChanged(s, e);
					}
				};
			}
		}

		protected override void OnRenderSizeChanged (SizeChangedInfo sizeInfo)
		{
			base.OnRenderSizeChanged (sizeInfo);
			Update (ViewModel.Value.Color);
		}

		void OnHueChanged (object s, MouseEventArgs e)
		{
			var position = e.GetPosition ((IInputElement)s);
			CurrentHue = GetHueFromPosition (position);
			var color = GetColorFromPosition (CurrentCursorPosition);
			UpdateCurrentColor (color.R, color.G, color.B, skipShadeUpdate: true);
		}

		void OnShadeChanged (object s, MouseEventArgs e)
		{
			CurrentCursorPosition = e.GetPosition ((IInputElement)s);
			var color = GetColorFromPosition (CurrentCursorPosition);
			UpdateCurrentColor (color.R, color.G, color.B, skipHueUpdate: true);
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
		/// <param name="position"></param>
		/// <returns></returns>
		CommonColor GetColorFromPosition (Point position)
		{
			var saturation = position.X / topShadeChooser.ActualWidth;
			var brightness = 1 - position.Y / topShadeChooser.ActualHeight;

			return new CommonColor (
				(byte)((255 + (CurrentHue.R - 255) * saturation) * brightness),
				(byte)((255 + (CurrentHue.G - 255) * saturation) * brightness),
				(byte)((255 + (CurrentHue.B - 255) * saturation) * brightness),
				255
			);
		}

		/// <summary>
		/// Finds a position on the shade chooser that corresponds to the passed-in
		/// color.
		/// </summary>
		/// <param name="color"></param>
		/// <returns></returns>
		Point GetPositionFromColor(CommonColor color, CommonColor hue)
		{
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
				xrate * topShadeChooser.ActualWidth + topShadeChooser.Margin.Left - cursorWidth / 2,
				(1 - alphaFactor) * topShadeChooser.ActualHeight + topShadeChooser.Margin.Top - cursorHeight / 2);
		}

		int[][] redRanges = new[] { new[] { 0, 1 }, new[] { 5, 6 } };
		int[][] greenRanges = new[] { new[] { 1, 3 } };
		int[][] blueRanges = new[] { new[] { 3, 5 } };

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
		/// <param name="position">The horizontal position on the hue picker</param>
		/// <returns>The hue</returns>
		CommonColor GetHueFromPosition (Point position)
		{
			var dialPosition = Math.Min(Math.Max(0, position.X * 6 / hueChooser.ActualWidth), 6);
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
		byte GetHueComponent (double position, int[][] intervals)
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
		/// Gets a hue from a color.
		/// A hue has the highest component of the passed-in color at 255,
		/// the lowest at 0, and the intermediate one is interpolated.
		/// The result is a maximally saturated and bright color that looks
		/// like the original color.
		/// The precision of the mappin goes down as the color gets darker.
		/// All shades of grey get arbitrarily mapped to red.
		/// </summary>
		/// <param name="color">The color for which to find the hue</param>
		/// <returns>The hue</returns>
		CommonColor GetHueFromColor (CommonColor color)
		{
			// Map grey to red
			if (color.R == color.G && color.G == color.B)
				return new CommonColor (255, 0, 0);

			var isRedMax = color.R >= color.G && color.R >= color.B;
			var isGreenMax = color.G >= color.R && color.G >= color.B;
			var isRedMin = color.R <= color.G && color.R <= color.B;
			var isGreenMin = color.G <= color.R && color.G <= color.B;
			if (isRedMax) {
				if (isGreenMin)
					return new CommonColor (255, 0, InterpolateComponent (color.B, color.G, color.R));
				else // blue is min
					return new CommonColor (255, InterpolateComponent (color.G, color.B, color.R), 0);
			}
			if (isGreenMax) {
				if (isRedMin)
					return new CommonColor (0, 255, InterpolateComponent (color.B, color.R, color.G));
				else // blue is min
					return new CommonColor (InterpolateComponent (color.R, color.B, color.G), 255, 0);
			}
			// blue is max
			if (isRedMin)
				return new CommonColor (0, InterpolateComponent (color.G, color.R, color.B), 255);
			else // green is min
				return new CommonColor (InterpolateComponent (color.R, color.G, color.B), 0, 255);
		}

		/// <summary>
		/// Computes where the third component should be if the top is mapped
		/// to 255 and the lowest gets mapped to 0.
		/// </summary>
		/// <param name="component">The third component's value</param>
		/// <param name="lowest">The lowest component value</param>
		/// <param name="highest">The highest component value</param>
		/// <returns>The interpolated third component</returns>
		byte InterpolateComponent(byte component, byte lowest, byte highest)
		{
			var delta = highest - lowest;
			if (delta == 0) return highest;
			return (byte)((component - lowest) * 255 / delta);
		}

		/// <summary>
		/// Updates the view model, as well as the hue and shade controls
		/// to reflect the values of the text boxes.
		/// </summary>
		void UpdateFromTextBoxes ()
		{
			var oldColor = ViewModel.Value.Color;
			var newColor = new CommonColor (oldColor.R, oldColor.G, oldColor.B, oldColor.A);
			if (byte.TryParse (redEntry.Text, out byte newRed)) {
				newColor.R = newRed;
			}
			if (byte.TryParse (greenEntry.Text, out byte newGreen)) {
				newColor.G = newGreen;
			}
			if (byte.TryParse (blueEntry.Text, out byte newBlue)) {
				newColor.B = newBlue;
			}
			if (byte.TryParse (alphaEntry.Text, out byte newAlpha)) {
				newColor.A = newAlpha;
			}
			Update (newColor);
		}

		/// <summary>
		/// Updates the current color, leaving the current alpha channel and hue unchanged.
		/// </summary>
		/// <param name="red">The red component of the new color</param>
		/// <param name="green">The green component of the new color</param>
		/// <param name="blue">The blue component of the new color</param>
		/// <param name="skipHueUpdate">If true, skips updating the current hue and associated UI</param>
		void UpdateCurrentColor (byte red, byte green, byte blue, bool skipHueUpdate = false, bool skipShadeUpdate = false)
		{
			var alpha = ViewModel.Value.Color.A;
			Update (new CommonColor (red, green, blue, alpha), skipHueUpdate, skipShadeUpdate);
		}

		/// <summary>
		/// Updates the view model with a new color, then updates the UI accordingly.
		/// </summary>
		/// <param name="newColor">The new color</param>
		/// <param name="skipHueUpdate">If true, skips updating the current hue and associated UI</param>
		void Update (CommonColor newColor, bool skipHueUpdate = false, bool skipShadeUpdate = false)
		{
			ViewModel.Value = new CommonSolidBrush (newColor);

			// Update the text boxes
			redEntry.Text = newColor.R.ToString ();
			greenEntry.Text = newColor.G.ToString ();
			blueEntry.Text = newColor.B.ToString ();
			alphaEntry.Text = newColor.A.ToString ();

			// Update the shade picker's gradient to reflect the current hue
			if (!skipHueUpdate) {
				CurrentHue = GetHueFromColor (newColor);
			}
			var newBrush = (LinearGradientBrush)bottomShadeChooser.Fill.Clone ();
			var gradientStops = newBrush.GradientStops;
			gradientStops.RemoveAt (1);
			gradientStops.Add (new GradientStop (Color.FromRgb (CurrentHue.R, CurrentHue.G, CurrentHue.B), 1));
			bottomShadeChooser.Fill = newBrush;

			if (!skipShadeUpdate) {
				// Move the shade picker's cursor to the current color
				CurrentCursorPosition = GetPositionFromColor (newColor, CurrentHue);
				foreach (var child in cursor.Children) {
					var shape = child as Shape;
					if (shape != null) {
						Canvas.SetTop (shape, CurrentCursorPosition.Y);
						Canvas.SetLeft (shape, CurrentCursorPosition.X);
					}
				}
			}
		}

		static readonly Regex digitsOnly = new Regex ("^[0-9]?$");
		void ConstrainToDigits (object sender, TextCompositionEventArgs e)
		{
			e.Handled = !digitsOnly.IsMatch (e.Text);
		}
	}
}
