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

		// TODO: make this its own property view model so we can edit bindings, set to resources, etc.
		public string ColorSpace => Parent.Value is CommonSolidBrush solidBrush ? solidBrush.ColorSpace : null;

		public CommonColor HueColor {
			get => (this.hueColor ?? (this.hueColor = LastColor.HueColor)).Value;
			set {
				if (!this.hueColor.Equals (value)) {
					this.hueColor = value;
					OnPropertyChanged ();

					var saturation = Shade.Saturation;
					// We should not update the color value on a hue change if the current color is unsaturated, as grey has no hue.
					if (saturation != 0) {
						var brightness = Color.Brightness;
						var newColor = CommonColor.FromHSB (value.Hue, saturation, brightness, Color.A);
						Parent.Value = new CommonSolidBrush (newColor, ColorSpace, Parent.Value.Opacity);
					}
				}
			}
		}

		public CommonColor Shade {
			get => (this.shade ?? (this.shade = LastColor)).Value;
			set {
				if (!this.shade.Equals (value)) {
					this.shade = value;
					OnPropertyChanged ();
					Parent.Value = new CommonSolidBrush (
						new CommonColor(value.R, value.G, value.B, Color.A),
						ColorSpace, Parent.Value?.Opacity ?? 1);
				}
			}
		}

		// TODO: make this its own property view model so we can edit bindings, set to resources, etc.
		public CommonColor Color {
			get => Parent.Value is CommonSolidBrush solidBrush ? solidBrush.Color : new CommonColor (0, 0, 0);
			set {
				if (!Color.Equals (value)) {
					Parent.Value = new CommonSolidBrush (value, ColorSpace, Parent.Value?.Opacity ?? 1);
					OnPropertyChanged ();
					if (!this.initialColor.HasValue) {
						this.initialColor = value;
					}
				}
			}
		}

		public CommonColor InitialColor => this.initialColor ?? (this.initialColor = Color).Value;

		public CommonColor LastColor => this.lastColor ?? (this.lastColor = Color).Value;

		public void CommitLastColor ()
		{
			if (this.lastColor == Color) return;
			this.lastColor = Color;
			OnPropertyChanged (nameof (LastColor));
			this.shade = null;
			OnPropertyChanged (nameof (Shade));
			var opacity = Parent.Value != null ? Parent.Value.Opacity : 1.0;
			Parent.Value = new CommonSolidBrush (Color, ColorSpace, opacity);
		}

		public void CommitHue ()
		{
			this.hueColor = null;
			OnPropertyChanged (nameof (HueColor));
		}

		public void ResetInitialColor()
		{
			this.initialColor = null;
			OnPropertyChanged (nameof (InitialColor));
		}

		private BrushPropertyViewModel Parent { get; }

		private CommonColor? hueColor;
		private CommonColor? shade;
		private CommonColor? initialColor;
		private CommonColor? lastColor;

		private void Parent_PropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof (BrushPropertyViewModel.Value)) {
				OnPropertyChanged (nameof (Color));
				OnPropertyChanged (nameof (ColorSpace));
			}
		}
	}
}
