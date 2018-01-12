using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class SolidBrushViewModel : NotifyingObject
	{
		public SolidBrushViewModel (BrushPropertyViewModel parent, IReadOnlyList<string> colorSpaces = null)
		{
			Parent = parent ?? throw new ArgumentNullException (nameof (parent));
			ColorSpaces = colorSpaces;
			parent.PropertyChanged += Parent_PropertyChanged;
		}

		public IReadOnlyList<string> ColorSpaces { get; }

		public CommonSolidBrush PreviousSolidBrush { get; set; }

		public ValueInfo<string> ColorSpace
		{
			get => this.colorSpace ?? (this.colorSpace = new ValueInfo<string> {
				Source = ValueSource.Local,
				Value = Parent.Value is CommonSolidBrush solidBrush ? solidBrush.ColorSpace : null
			});
			set {
				this.colorSpace = value;
				OnPropertyChanged ();
				SetParentValue (Color.Value, value.Value, Parent.Value.Opacity);
			}
		}

		public CommonColor HueColor {
			get => (this.hueColor ?? (this.hueColor = LastColor.HueColor)).Value;
			set {
				if (!this.hueColor.Equals (value)) {
					var saturation = Color.Value.Saturation;
					var brightness = Color.Value.Brightness;
					Color = new ValueInfo<CommonColor> {
						Source = ValueSource.Local,
						Value = CommonColor.FromHSB (value.Hue, saturation, brightness, Color.Value.A)
					};
					this.hueColor = value;
					OnPropertyChanged ();
					SetParentValue (Color.Value, ColorSpace.Value, Parent.Value.Opacity);
				}
			}
		}

		public CommonColor Shade {
			get => (this.shade.HasValue ? this.shade : (this.shade = LastColor)).Value;
			set {
				if (!this.shade.Equals (value)) {
					this.shade = value;
					OnPropertyChanged ();
					SetParentValue (value, ColorSpace.Value, Parent.Value.Opacity);
				}
			}
		}

		public ValueInfo<CommonColor> Color {
			get => this.color ?? (
				this.color = new ValueInfo<CommonColor> {
					Source = ValueSource.Local,
					Value = Parent.Value is CommonSolidBrush solidBrush ? solidBrush.Color : new CommonColor (0, 0, 0)
				});
			set {
				if (!Color.Equals (value)) {
					this.color = value;
					CommonColor oldHue = HueColor;
					CommonColor newColor = value.Value;
					CommonColor newHue = newColor.HueColor;
					SetParentValue (newColor, ColorSpace.Value, Parent.Value.Opacity);
					OnPropertyChanged ();
					if (!newHue.Equals (oldHue)) {
						this.hueColor = newHue;
						OnPropertyChanged (nameof (HueColor));
					}
					if (!newColor.Equals (this.shade)) {
						this.shade = newColor;
						OnPropertyChanged (nameof (Shade));
					}
					if (!this.initialColor.HasValue) {
						this.initialColor = newColor;
					}
				}
			}
		}

		public CommonColor InitialColor => this.initialColor ?? (this.initialColor = Color.Value).Value;

		public CommonColor LastColor => this.lastColor ?? (this.lastColor = Color.Value).Value;

		public void CommitLastColor ()
		{
			this.lastColor = Color.Value;
			this.shade = Color.Value;
			this.hueColor = Color.Value.HueColor;
			var opacity = Parent.Value != null ? Parent.Value.Opacity : 1.0;
			OnPropertyChanged (nameof (LastColor));
			OnPropertyChanged (nameof (Shade));
			OnPropertyChanged (nameof (HueColor));
			SetParentValue (Color.Value, ColorSpace.Value, opacity);
		}

		public void CommitShade ()
		{
			this.lastColor = Shade;
			var opacity = Parent.Value != null ? Parent.Value.Opacity : 1.0;
			OnPropertyChanged (nameof (LastColor));
			SetParentValue (Shade, ColorSpace.Value, opacity);
		}

		public void SetParentValue (CommonColor color, string colorSpace, double opacity)
		{
			Parent.Value = new CommonSolidBrush (color, colorSpace, opacity);
			// If the new value corresponds to a different color, we want to reset the Color value info,
			// but not otherwise.
			if (this.color != null && this.color.Value != color) {
				this.color = null;
			}
		}

		BrushPropertyViewModel Parent { get; }

		CommonColor? hueColor;
		CommonColor? shade;
		CommonColor? initialColor;
		CommonColor? lastColor;

		ValueInfo<string> colorSpace;
		ValueInfo<CommonColor> color;

		private void Parent_PropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof (BrushPropertyViewModel.Value)) {
				OnPropertyChanged (nameof (Color));
				OnPropertyChanged (nameof (ColorSpace));
			}
		}
	}
}
