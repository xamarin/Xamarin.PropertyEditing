using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
		HueEditorControl hueChooser;
		ShadeEditorControl shadeChooser;

		SolidBrushPropertyViewModel ViewModel => DataContext as SolidBrushPropertyViewModel;

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
			shadeChooser = (ShadeEditorControl)GetTemplateChild ("shadeChooser");

			if (ViewModel.Property.CanWrite) {
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
			}
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
		void UpdateCurrentColor (byte red, byte green, byte blue, bool skipHueUpdate = false)
		{
			var alpha = ViewModel.Value.Color.A;
			Update (new CommonColor (red, green, blue, alpha), skipHueUpdate);
		}

		/// <summary>
		/// Updates the view model with a new color, then updates the UI accordingly.
		/// </summary>
		/// <param name="newColor">The new color</param>
		/// <param name="skipHueUpdate">If true, skips updating the current hue and associated UI</param>
		void Update (CommonColor newColor, bool skipHueUpdate = false)
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
		}

		static readonly Regex digitsOnly = new Regex ("^[0-9]?$");
		void ConstrainToDigits (object sender, TextCompositionEventArgs e)
		{
			e.Handled = !digitsOnly.IsMatch (e.Text);
		}
	}
}
