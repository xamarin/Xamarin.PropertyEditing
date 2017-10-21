using System.Collections.Generic;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class SolidBrushPropertyViewModel : PropertyViewModel<CommonSolidBrush>, IColorSpaced
	{
		public SolidBrushPropertyViewModel(IPropertyInfo property, IEnumerable<IObjectEditor> editors)
			: base(property, editors)
		{
			var solidBrushPropertyInfo = property as IColorSpaced;
			if (solidBrushPropertyInfo != null) {
				ColorSpaces = solidBrushPropertyInfo.ColorSpaces;
			}
		}

		public IReadOnlyList<string> ColorSpaces { get; }

		CommonColor? hue;
		public CommonColor Hue {
			get => (hue.HasValue ? hue : (hue = LastColor.Hue)).Value;
			set {
				if (!hue.Equals(value)) {
					var luminosity = Color.Luminosity;
					var saturation = Color.Saturation;
					Color = CommonColor.From (value, luminosity, saturation, Color.A);
					OnPropertyChanged (nameof (Color));
					hue = value;
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
					var oldHue = Hue;
					var newHue = value.Hue;
					Value = new CommonSolidBrush (value, Value.ColorSpace, Value.Opacity);
					OnPropertyChanged ();
					if (!newHue.Equals(oldHue)) {
						hue = newHue;
						OnPropertyChanged (nameof (Hue));
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
		public CommonColor InitialColor {
			get => initialColor.HasValue ? initialColor.Value : (initialColor = Color).Value;
		}

		CommonColor? lastColor;
		public CommonColor LastColor {
			get => lastColor.HasValue ? lastColor.Value : (lastColor = Color).Value;
		}

		public void CommitLastColor()
		{
			lastColor = Color;
			shade = Color;
			hue = Color.Hue;
			OnPropertyChanged (nameof (LastColor));
			OnPropertyChanged (nameof (Shade));
			OnPropertyChanged (nameof (Hue));
			Value = new CommonSolidBrush (Color, Value.ColorSpace, Value.Opacity);
		}

		public void CommitShade ()
		{
			lastColor = Shade;
			OnPropertyChanged (nameof (LastColor));
			Value = new CommonSolidBrush (Shade, Value.ColorSpace, Value.Opacity);
		}

		protected override void OnValueChanged ()
		{
			base.OnValueChanged ();
			OnPropertyChanged (nameof (Color));
			OnPropertyChanged (nameof (Shade));
			OnPropertyChanged (nameof (Hue));
		}
	}
}
