using System.Collections.Generic;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class BrushPropertyViewModel : PropertyViewModel<CommonBrush>
	{
		public BrushPropertyViewModel(IPropertyInfo property, IEnumerable<IObjectEditor> editors)
			: base(property, editors)
		{
			if (property is IColorSpaced colorSpacedPropertyInfo) {
				ColorSpaces = colorSpacedPropertyInfo.ColorSpaces;
			}
		}

		public IReadOnlyList<string> ColorSpaces { get; }

		public CommonSolidBrush PreviousSolidBrush { get; set; }

		string ColorSpace => Value is CommonSolidBrush solidBrush ? solidBrush.ColorSpace : null;

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
					Value = new CommonSolidBrush (Color, ColorSpace, Value.Opacity);
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
					Value = new CommonSolidBrush (value, ColorSpace, Value.Opacity);
				}
			}
		}

		public CommonColor Color {
			get => Value is CommonSolidBrush solidBrush ? solidBrush.Color : new CommonColor(0, 0, 0);
			set {
				if (!Color.Equals (value)) {
					CommonColor oldHue = HueColor;
					CommonColor newHue = value.HueColor;
					Value = new CommonSolidBrush (value, ColorSpace, Value.Opacity);
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
			OnPropertyChanged (nameof (LastColor));
			OnPropertyChanged (nameof (Shade));
			OnPropertyChanged (nameof (HueColor));
			Value = new CommonSolidBrush (Color, ColorSpace, Value.Opacity);
		}

		public void CommitShade ()
		{
			lastColor = Shade;
			OnPropertyChanged (nameof (LastColor));
			Value = new CommonSolidBrush (Shade, ColorSpace, Value.Opacity);
		}
	}
}
