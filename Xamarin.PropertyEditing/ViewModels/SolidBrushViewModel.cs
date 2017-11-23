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
					var saturation = Color.Saturation;
					var brightness = Color.Brightness;
					Color = CommonColor.FromHSB (value.Hue, saturation, brightness, Color.A);
					this.hueColor = value;
					OnPropertyChanged ();
					Parent.Value = new CommonSolidBrush (Color, ColorSpace, Parent.Value.Opacity);
				}
			}
		}

		public CommonColor Shade {
			get => (this.shade.HasValue ? this.shade : (this.shade = LastColor)).Value;
			set {
				if (!this.shade.Equals (value)) {
					this.shade = value;
					OnPropertyChanged ();
					Parent.Value = new CommonSolidBrush (value, ColorSpace, Parent.Value.Opacity);
				}
			}
		}

		// TODO: make this its own property view model so we can edit bindings, set to resources, etc.
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
			this.lastColor = Color;
			this.shade = Color;
			this.hueColor = Color.HueColor;
			var opacity = Parent.Value != null ? Parent.Value.Opacity : 1.0;
			OnPropertyChanged (nameof (LastColor));
			OnPropertyChanged (nameof (Shade));
			OnPropertyChanged (nameof (HueColor));
			Parent.Value = new CommonSolidBrush (Color, ColorSpace, opacity);
		}

		public void CommitShade ()
		{
			this.lastColor = Shade;
			var opacity = Parent.Value != null ? Parent.Value.Opacity : 1.0;
			OnPropertyChanged (nameof (LastColor));
			Parent.Value = new CommonSolidBrush (Shade, ColorSpace, opacity);
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
