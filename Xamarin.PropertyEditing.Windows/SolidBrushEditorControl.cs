using System;
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
				var insideHueChooser = false;
				hueChooser.MouseLeave += (s, e) => insideHueChooser = false;
				hueChooser.MouseLeftButtonDown += (s, e) => {
					insideHueChooser = true;
					var position = e.GetPosition ((IInputElement)s);
					CurrentHue = GetHueFromPosition (position);
					Update ();
				};
				hueChooser.MouseLeftButtonUp += (s, e) => insideHueChooser = false;
				hueChooser.MouseMove += (s, e) => {
					if (e.LeftButton == MouseButtonState.Pressed && insideHueChooser) {
						var position = e.GetPosition ((IInputElement)s);
						CurrentHue = GetHueFromPosition (position);
						Update ();
					}
				};

				// Handle interaction with the top shade chooser
				bool mouseInsideShadeChooser = false;
				topShadeChooser.MouseLeave += (s, e) => mouseInsideShadeChooser = false;
				topShadeChooser.MouseLeftButtonDown += (s, e) => {
					mouseInsideShadeChooser = true;
					CurrentCursorPosition = e.GetPosition ((IInputElement)s);
					var color = GetColorFromPosition (CurrentCursorPosition);
					UpdateCurrentColor (color.R, color.G, color.B);
				};
				topShadeChooser.MouseLeftButtonUp += (s, e) => mouseInsideShadeChooser = false;
				topShadeChooser.MouseMove += (s, e) => {
					CurrentCursorPosition = e.GetPosition ((IInputElement)s);
					if (e.LeftButton == MouseButtonState.Pressed && mouseInsideShadeChooser) {
						var color = GetColorFromPosition (CurrentCursorPosition);
						UpdateCurrentColor (color.R, color.G, color.B);
					}
				};
			}

			Update ();
		}

		const int DeltaShadeCoordinates = 0;
		CommonColor GetColorFromPosition (Point position)
		{
			var w = topShadeChooser.ActualWidth - DeltaShadeCoordinates;
			var h = topShadeChooser.ActualHeight - DeltaShadeCoordinates;

			var xrate = Math.Min (1, Math.Max (0, position.X - DeltaShadeCoordinates) / w);
			var alphaFactor = (float)(1 - Math.Min (1, Math.Max (0, position.Y - DeltaShadeCoordinates) / h));

			return new CommonColor (
				(byte)((255 + (CurrentHue.R - 255) * xrate) * alphaFactor),
				(byte)((255 + (CurrentHue.G - 255) * xrate) * alphaFactor),
				(byte)((255 + (CurrentHue.B - 255) * xrate) * alphaFactor),
				255
			);
		}

		Point GetPositionFromColor(CommonColor color)
		{
			var hue = GetHueFromColor(color);
			var isRedMaxed = hue.R == 255;
			var isGreenMaxed = hue.G == 255;
			var alphaFactor = isRedMaxed ? (double)color.R / 255 : isGreenMaxed ? (double)color.G / 255 : (double)color.B / 255;
			var isRedMin = hue.R <= hue.G && hue.R <= hue.B;
			var isGreenMin = hue.G <= hue.R && hue.G <= hue.B;
			var xrate = isRedMin ?
				(color.R / alphaFactor - 255) / (hue.R - 255)
				: isGreenMin ?
				(color.G / alphaFactor - 255) / (hue.G - 255)
				: (color.B / alphaFactor - 255) / (hue.B - 255);
			return new Point (
				xrate * topShadeChooser.ActualWidth + DeltaShadeCoordinates,
				(1 - alphaFactor) * topShadeChooser.ActualHeight + DeltaShadeCoordinates);
		}

		// The hue dial is a gradient going through red, yellow, lime, cyan, blue, magenta, red.
		// This means the following variations for red, green, and blue components:
		//      0 |   1 |   2 |   3 |   4 |   5 |   6
		// -------|-----|-----|-----|-----|-----|-----
		// R: 255 | 255 |   0 |   0 |   0 | 255 | 255
		// G:   0 | 255 | 255 | 255 |   0 |   0 |   0
		// B:   0 |   0 |   0 | 255 | 255 | 255 |   0
		int[][] redRanges = new[] { new[] { 0, 1 }, new[] { 5, 6 } };
		int[][] greenRanges = new[] { new[] { 1, 3 } };
		int[][] blueRanges = new[] { new[] { 3, 5 } };

		CommonColor GetHueFromPosition (Point position)
		{
			var dialPosition = position.X * 6 / hueChooser.ActualWidth;
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
			if (position < 0 || position >= 6)
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

		CommonColor GetHueFromColor (CommonColor color)
		{
			if (color.R == 0 && color.G == 0 && color.B == 0)
				return new CommonColor (255, 0, 0);
			double ratio;
			if (color.R >= color.G && color.R >= color.B) {
				ratio = 255.0 / color.R;
				return new CommonColor (255, (byte)(color.G * ratio), (byte)(color.B * ratio));
			}
			if (color.G >= color.R && color.G >= color.B) {
				ratio = 255.0 / color.G;
				return new CommonColor ((byte)(color.R * ratio), 255, (byte)(color.B * ratio));
			}
			ratio = 255.0 / color.B;
			return new CommonColor ((byte)(color.R * ratio), (byte)(color.G * ratio), 255);
		}

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
			UpdateHueAndCursorPosition(newColor);
			Update ();
		}

		void UpdateCurrentColor(byte red, byte green, byte blue)
		{
			var alpha = ViewModel.Value.Color.A;
			UpdateCurrentColor (new CommonColor (red, green, blue, alpha));
		}

		void UpdateCurrentColor (CommonColor newColor)
		{
			ViewModel.Value = new CommonSolidBrush (newColor);
			Update ();
		}

		void UpdateHueAndCursorPosition (CommonColor newColor)
		{
			CurrentHue = GetHueFromColor (newColor);
			ViewModel.Value = new CommonSolidBrush (newColor);
			CurrentCursorPosition = GetPositionFromColor (newColor);
		}

		void Update ()
		{
			var newColor = ViewModel.Value.Color;

			// Update the position of the cursor
			if (CurrentCursorPosition == default (Point)) {
				UpdateHueAndCursorPosition (newColor);
			}
			foreach (var child in cursor.Children) {
				var shape = child as Shape;
				if (shape != null) {
					Canvas.SetTop (shape, CurrentCursorPosition.Y);
					Canvas.SetLeft (shape, CurrentCursorPosition.X);
				}
			}

			// Update the text boxes
			redEntry.Text = newColor.R.ToString ();
			greenEntry.Text = newColor.G.ToString ();
			blueEntry.Text = newColor.B.ToString ();
			alphaEntry.Text = newColor.A.ToString ();

			// Update the shade chooser's gradient to reflect the current hue
			var newBrush = (LinearGradientBrush)bottomShadeChooser.Fill.Clone ();
			var gradientStops = newBrush.GradientStops;
			gradientStops.RemoveAt (1);
			gradientStops.Add (new GradientStop (Color.FromRgb (CurrentHue.R, CurrentHue.G, CurrentHue.B), 1));
			bottomShadeChooser.Fill = newBrush;
		}

		static readonly Regex digitsOnly = new Regex ("^[0-9]?$");
		void ConstrainToDigits (object sender, TextCompositionEventArgs e)
		{
			e.Handled = !digitsOnly.IsMatch (e.Text);
		}
	}
}
