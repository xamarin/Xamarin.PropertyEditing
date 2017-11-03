using System.Collections.Generic;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class SolidBrushViewModel : NotifyingObject
	{
		public SolidBrushViewModel (BrushPropertyViewModel parent, IReadOnlyList<string> colorSpaces = null)
		{
			Parent = parent;
			ColorSpaces = colorSpaces;
		}

		BrushPropertyViewModel Parent { get; }

		public IReadOnlyList<string> ColorSpaces { get; }

		public CommonSolidBrush PreviousSolidBrush { get; set; }

		string ColorSpace => Parent.Value is CommonSolidBrush solidBrush ? solidBrush.ColorSpace : null;

		CommonColor? hueColor;
		public CommonColor HueColor {
			get => (hueColor ?? (hueColor = LastColor.HueColor)).Value;
			set {
				if (!hueColor.Equals (value)) {
					var saturation = Color.Saturation;
					var brightness = Color.Brightness;
					Color = CommonColor.FromHSB (value.Hue, saturation, brightness, Color.A);
					OnPropertyChanged (nameof (Color));
					hueColor = value;
					OnPropertyChanged ();
					Parent.Value = new CommonSolidBrush (Color, ColorSpace, Parent.Value.Opacity);
				}
			}
		}

		CommonColor? shade;
		public CommonColor Shade {
			get => (shade.HasValue ? shade : (shade = LastColor)).Value;
			set {
				if (!shade.Equals (value)) {
					shade = value;
					OnPropertyChanged ();
					Parent.Value = new CommonSolidBrush (value, ColorSpace, Parent.Value.Opacity);
				}
			}
		}

		public CommonColor Color {
			get => Parent.Value is CommonSolidBrush solidBrush ? solidBrush.Color : new CommonColor (0, 0, 0);
			set {
				if (!Color.Equals (value)) {
					CommonColor oldHue = HueColor;
					CommonColor newHue = value.HueColor;
					Parent.Value = new CommonSolidBrush (value, ColorSpace, Parent.Value.Opacity);
					OnPropertyChanged ();
					if (!newHue.Equals (oldHue)) {
						hueColor = newHue;
						OnPropertyChanged (nameof (HueColor));
					}
					if (!value.Equals (shade)) {
						shade = value;
						OnPropertyChanged (nameof (Shade));
					}
					if (!initialColor.HasValue) {
						initialColor = value;
					}
				}
			}
		}

		CommonColor? initialColor;
		public CommonColor InitialColor => initialColor ?? (initialColor = Color).Value;

		CommonColor? lastColor;
		public CommonColor LastColor => lastColor ?? (lastColor = Color).Value;

		public void CommitLastColor ()
		{
			lastColor = Color;
			shade = Color;
			hueColor = Color.HueColor;
			var opacity = Parent.Value != null ? Parent.Value.Opacity : 1.0;
			OnPropertyChanged (nameof (LastColor));
			OnPropertyChanged (nameof (Shade));
			OnPropertyChanged (nameof (HueColor));
			Parent.Value = new CommonSolidBrush (Color, ColorSpace, opacity);
		}

		public void CommitShade ()
		{
			lastColor = Shade;
			var opacity = Parent.Value != null ? Parent.Value.Opacity : 1.0;
			OnPropertyChanged (nameof (LastColor));
			Parent.Value = new CommonSolidBrush (Shade, ColorSpace, opacity);
		}
	}
}
