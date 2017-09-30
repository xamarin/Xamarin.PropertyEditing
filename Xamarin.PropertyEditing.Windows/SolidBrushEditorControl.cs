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

		TextBox redEntry;
		TextBox greenEntry;
		TextBox blueEntry;
		TextBox alphaEntry;

		SolidBrushPropertyViewModel ViewModel => DataContext as SolidBrushPropertyViewModel;

		public override void OnApplyTemplate ()
		{
			base.OnApplyTemplate ();

			redEntry = (TextBox)GetTemplateChild ("redEntry");
			redEntry.PreviewTextInput += ConstrainToDigits;
			redEntry.LostFocus += (s, e) => Update();
			greenEntry = (TextBox)GetTemplateChild ("greenEntry");
			greenEntry.PreviewTextInput += ConstrainToDigits;
			greenEntry.LostFocus += (s, e) => Update ();
			blueEntry = (TextBox)GetTemplateChild ("blueEntry");
			blueEntry.PreviewTextInput += ConstrainToDigits;
			blueEntry.LostFocus += (s, e) => Update ();
			alphaEntry = (TextBox)GetTemplateChild ("alphaEntry");
			alphaEntry.PreviewTextInput += ConstrainToDigits;
			alphaEntry.LostFocus += (s, e) => Update ();
		}

		private void Update ()
		{
			var oldColor = ViewModel.Value.Color;
			var newColor = new CommonColor (oldColor.R, oldColor.G, oldColor.B, oldColor.A);
			if (byte.TryParse (redEntry.Text, out byte newRed)) {
				newColor.R = newRed;
			}
			else {
				redEntry.Text = oldColor.R.ToString();
			}
			if (byte.TryParse (greenEntry.Text, out byte newGreen)) {
				newColor.G = newGreen;
			}
			else {
				greenEntry.Text = oldColor.G.ToString ();
			}
			if (byte.TryParse (blueEntry.Text, out byte newBlue)) {
				newColor.B = newBlue;
			}
			else {
				blueEntry.Text = oldColor.B.ToString ();
			}
			if (byte.TryParse (alphaEntry.Text, out byte newAlpha)) {
				newColor.A = newAlpha;
			}
			else {
				alphaEntry.Text = oldColor.A.ToString ();
			}
			ViewModel.Value = new CommonSolidBrush (newColor);
		}

		static readonly Regex digitsOnly = new Regex ("^[0-9]?$");
		private void ConstrainToDigits (object sender, TextCompositionEventArgs e)
		{
			e.Handled = !digitsOnly.IsMatch(e.Text);
		}
	}
}
