using System.ComponentModel;
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

		ComboBox colorSpacePicker;
		TextBox redEntry;
		TextBox greenEntry;
		TextBox blueEntry;
		TextBox alphaEntry;
		Rectangle topShadeChooser;
		Rectangle bottomShadeChooser;
		HueEditorControl hueChooser;
		Canvas cursor;
		double cursorWidth;
		double cursorHeight;

		SolidBrushPropertyViewModel ViewModel => DataContext as SolidBrushPropertyViewModel;

		Point CurrentCursorPosition;

		public override void OnApplyTemplate ()
		{
			base.OnApplyTemplate ();

			colorSpacePicker = (ComboBox)GetTemplateChild ("colorSpacePicker");
			if (ViewModel.ColorSpaces == null || ViewModel.ColorSpaces.Count == 0) {
				colorSpacePicker.Visibility = Visibility.Collapsed;
			}

			redEntry = (TextBox)GetTemplateChild ("redEntry");
			greenEntry = (TextBox)GetTemplateChild ("greenEntry");
			blueEntry = (TextBox)GetTemplateChild ("blueEntry");
			alphaEntry = (TextBox)GetTemplateChild ("alphaEntry");
			hueChooser = (HueEditorControl)GetTemplateChild ("hueChooser");
			topShadeChooser = (Rectangle)GetTemplateChild ("topShadeChooser");
			bottomShadeChooser = (Rectangle)GetTemplateChild ("bottomShadeChooser");
			cursor = (Canvas)GetTemplateChild ("cursor");
			cursorWidth = (cursor.Children.Cast<Shape> ()).Max (c => c.Width);
			cursorHeight = (cursor.Children.Cast<Shape> ()).Max (c => c.Height);

			if (ViewModel.Property.CanWrite) {
				// Handle changes on the view model
				ViewModel.PropertyChanged += ViewModel_PropertyChanged;

				// Handle color space changes
				colorSpacePicker.SelectionChanged += (s, e) => {
					if (ViewModel != null && ViewModel.Value != null) {
						ViewModel.Value = new CommonSolidBrush (ViewModel.Value.Color, (string)e.AddedItems[0]);
					}
				};

				// Handle changes on ARGB text boxes
				redEntry.PreviewTextInput += ConstrainToDigits;
				redEntry.LostFocus += (s, e) => UpdateFromTextBoxes ();
				greenEntry.PreviewTextInput += ConstrainToDigits;
				greenEntry.LostFocus += (s, e) => UpdateFromTextBoxes ();
				blueEntry.PreviewTextInput += ConstrainToDigits;
				blueEntry.LostFocus += (s, e) => UpdateFromTextBoxes ();
				alphaEntry.PreviewTextInput += ConstrainToDigits;
				alphaEntry.LostFocus += (s, e) => UpdateFromTextBoxes ();

				// Handle interaction with the top shade chooser
				topShadeChooser.MouseLeftButtonDown += OnShadeChanged;
				topShadeChooser.MouseMove += (s, e) => {
					if (e.LeftButton == MouseButtonState.Pressed) {
						OnShadeChanged(s, e);
					}
				};
			}
		}

		private void ViewModel_PropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			// Handle interaction with the hue chooser
			if (e.PropertyName == "Hue") {
				var color = GetColorFromPosition (CurrentCursorPosition);
				UpdateCurrentColor (color.R, color.G, color.B, skipHueUpdate: true);
			}
		}

		protected override void OnRenderSizeChanged (SizeChangedInfo sizeInfo)
		{
			base.OnRenderSizeChanged (sizeInfo);
			Update (ViewModel.Value.Color, skipHueUpdate: true);
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
				(byte)((255 + (ViewModel.Hue.R - 255) * saturation) * brightness),
				(byte)((255 + (ViewModel.Hue.G - 255) * saturation) * brightness),
				(byte)((255 + (ViewModel.Hue.B - 255) * saturation) * brightness),
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
				ViewModel.Hue = newColor.ToHue();
			}
			var newBrush = (LinearGradientBrush)bottomShadeChooser.Fill.Clone ();
			var gradientStops = newBrush.GradientStops;
			gradientStops.RemoveAt (1);
			var currentHue = ViewModel.Hue;
			gradientStops.Add (new GradientStop (Color.FromRgb (currentHue.R, currentHue.G, currentHue.B), 1));
			bottomShadeChooser.Fill = newBrush;

			if (!skipShadeUpdate) {
				// Move the shade picker's cursor to the current color
				CurrentCursorPosition = GetPositionFromColor (newColor, currentHue);
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
