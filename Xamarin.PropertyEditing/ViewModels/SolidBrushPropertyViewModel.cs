using System.Collections.Generic;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class SolidBrushPropertyViewModel : PropertyViewModel<CommonSolidBrush>, IColorSpaced
	{
		public SolidBrushPropertyViewModel(IPropertyInfo property, IEnumerable<IObjectEditor> editors)
			: base(property, editors)
		{
			if (property is IColorSpaced solidBrushPropertyInfo) {
				ColorSpaces = solidBrushPropertyInfo.ColorSpaces;
			}
		}

		public IReadOnlyList<string> ColorSpaces { get; }

		CommonColor? hueColor;
		public CommonColor HueColor {
			get => (hueColor ?? (hueColor = LastColor.HueColor)).Value;
			set {
				if (!hueColor.Equals(value)) {
					var saturation = Color.Saturation;
					var brightness = Color.Brightness;
					Color = CommonColor.FromHSB (value.Hue, saturation, brightness, Color.A);
					OnPropertyChanged (nameof (Color));
					hueColor = value;
					OnPropertyChanged ();
					Value = new CommonSolidBrush(Color, Value.ColorSpace, Value.Opacity);
				}
			}
		}

		CommonColor? shade;
		public CommonColor Shade {
			get => (shade.HasValue ? shade : (shade = LastColor)).Value;
			set {
				if (!shade.Equals(value)) {
					shade = value;
					OnPropertyChanged ();
					Value = new CommonSolidBrush (value, Value.ColorSpace, Value.Opacity);
				}
			}
		}

		public CommonColor Color {
			get => Value.Color;
			set {
				if (!Value.Color.Equals(value)) {
					CommonColor oldHue = HueColor;
					CommonColor newHue = value.HueColor;
					Value = new CommonSolidBrush (value, Value.ColorSpace, Value.Opacity);
					OnPropertyChanged ();
					if (!newHue.Equals(oldHue)) {
						hueColor = newHue;
						OnPropertyChanged (nameof (HueColor));
					}
					if (!value.Equals(shade)) {
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

		public void CommitLastColor()
		{
			lastColor = Color;
			shade = Color;
			hueColor = Color.HueColor;
			OnPropertyChanged (nameof (LastColor));
			OnPropertyChanged (nameof (Shade));
			OnPropertyChanged (nameof (HueColor));
			Value = new CommonSolidBrush (Color, Value.ColorSpace, Value.Opacity);
		}

		public void CommitShade ()
		{
			lastColor = Shade;
			OnPropertyChanged (nameof (LastColor));
			Value = new CommonSolidBrush (Shade, Value.ColorSpace, Value.Opacity);
		}
	}
}
