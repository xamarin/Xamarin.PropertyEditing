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
			get => hue.HasValue ? hue.Value : (hue = Color.Hue).Value;
			set {
				if (!hue.Equals(value)) {
					hue = value;
					OnPropertyChanged ();
					var luminosity = Color.Luminosity;
					var saturation = Color.Saturation;
					Value = new CommonSolidBrush(CommonColor.From (value, luminosity, saturation, Color.A), Value.ColorSpace, Value.Opacity);
					OnPropertyChanged (nameof (Color));
				}
			}
		}

		public CommonColor Color {
			get => Value.Color;
			set {
				if (!Value.Color.Equals(value)) {
					var oldHue = Value.Color.Hue;
					var newHue = value.Hue;
					Value = new CommonSolidBrush (value, Value.ColorSpace, Value.Opacity);
					OnPropertyChanged ();
					if (!newHue.Equals(oldHue)) {
						OnPropertyChanged (nameof (Hue));
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
			OnPropertyChanged (nameof (LastColor));
		}

		protected override void OnValueChanged ()
		{
			base.OnValueChanged ();
			OnPropertyChanged (nameof (Color));
			hue = Color.Hue;
			OnPropertyChanged (nameof (Hue));
		}
	}
}
