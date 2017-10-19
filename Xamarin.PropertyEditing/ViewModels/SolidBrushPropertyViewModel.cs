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
			get => hue.HasValue ? hue.Value : (hue = Value.Color.Hue).Value;
			set {
				if (!hue.Equals(value)) {
					hue = value;
					OnPropertyChanged ();
					var luminosity = Value.Color.Luminosity;
					var saturation = Value.Color.Saturation;
					Value = new CommonSolidBrush(CommonColor.From (value, luminosity, saturation, Value.Color.A), Value.ColorSpace, Value.Opacity);
					OnPropertyChanged (nameof (Color));
				}
			}
		}

		public CommonColor Color {
			get => Value.Color;
			set {
				if (!Value.Color.Equals(value)) {
					Value = new CommonSolidBrush (value, Value.ColorSpace, Value.Opacity);
					OnPropertyChanged ();
					hue = value.Hue;
					OnPropertyChanged (nameof (Hue));
				}
			}
		}
	}
}
