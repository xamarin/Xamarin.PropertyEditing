using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Xamarin.PropertyEditing.Drawing;
using Strings = Xamarin.PropertyEditing.Properties.Resources;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class MaterialDesignColorViewModel : NotifyingObject
	{
		public MaterialDesignColorViewModel (BrushPropertyViewModel parent)
		{
			Parent = parent ?? throw new ArgumentNullException (nameof (parent));
			parent.PropertyChanged += Parent_PropertyChanged;
		}

		public CommonColor Color
		{
			get => Parent.Value is CommonSolidBrush solidBrush ? solidBrush.Color : new CommonColor (0, 0, 0);
			private set {
				if (!Color.Equals (value)) {
					Parent.Value = new CommonSolidBrush (value, null, Parent.Value.Opacity);
					OnPropertyChanged (nameof (Alpha));
					OnPropertyChanged ();
				}
			}
		}

		public string ColorName
		{
			get {
				if (this.colorName != null) return this.colorName;
				if (Parent.Value is CommonSolidBrush solidBrush) {
					return (this.colorName = Scale.Name);
				}
				return (this.colorName = Strings.MaterialColorGrey);
			}
			set {
				// Attempt to retain normal and accent selections
				if (NormalColor != null) {
					var normalColorIndex = Array.IndexOf (NormalColorScale.ToArray (), NormalColor);
					this.colorName = value;
					CommonColor?[] newScale = NormalColorScale.ToArray();
					if (newScale.Length > normalColorIndex) {
						NormalColor = newScale[normalColorIndex];
					}
					ReplaceColor (NormalColor.Value);
				} else if (AccentColor != null) {
					var accentColorIndex = Array.IndexOf (AccentColorScale.ToArray (), AccentColor);
					this.colorName = value;
					CommonColor?[] newScale = AccentColorScale.ToArray ();
					if (newScale.Length > accentColorIndex) {
						AccentColor = newScale[accentColorIndex];
					}
					ReplaceColor (AccentColor.Value);
				} else {
					this.colorName = value;
				}
				OnPropertyChanged ();
				OnPropertyChanged (nameof (Scale));
				OnPropertyChanged (nameof (AccentColorScale));
				OnPropertyChanged (nameof (NormalColorScale));
			}
		}

		public byte Alpha
		{
			get => Parent.Value is CommonSolidBrush solidBrush ? solidBrush.Color.A : (byte)255;
			set {
				CommonColor oldColor = Color;
				Color = new CommonColor (oldColor.R, oldColor.G, oldColor.B, value);
			}
		}

		public MaterialColorScale Scale
			=> MaterialPalettes
				.OrderBy (palette => palette.Colors.Min (
					paletteColor => CommonColor.SquaredDistance (paletteColor, Color)))
				.First ();

		public CommonColor? NormalColor
		{
			get {
				if (this.normalColor.HasValue) return this.normalColor.Value;
				MaterialColorScale scale = FindPalette (ColorName, false);
				foreach (CommonColor normalColor in scale.Colors) {
					if (Color.Equals (normalColor, true)) return (this.normalColor = normalColor).Value;
				}
				return null;
			}
			set {
				Debug.Assert (value == null || value.Value.A == 255, "NormalColor should never be set with a transparent color.");
				Debug.Assert (value == null || MaterialPalettes.Where (p => !p.IsAccent).SelectMany (p => p.Colors).Contains (value.Value), "NormalColor values should exist in the Material Design palette.");
				this.normalColor = value;
				if (ColorName != null && value.HasValue) {
					ReplaceColor (value.Value);
				}
				if (value.HasValue) {
					this.accentColor = null;
					OnPropertyChanged (nameof (AccentColor));
				}
				OnPropertyChanged ();
			}
		}

		public CommonColor? AccentColor
		{
			get {
				if (this.accentColor.HasValue) return this.accentColor.Value;
				MaterialColorScale scale = FindPalette (ColorName, true);
				foreach (CommonColor accent in scale.Colors) {
					if (Color.Equals (accent, true)) return (this.accentColor = accent).Value;
				}
				return null;
			}
			set {
				Debug.Assert (value?.A == 255, "AccentColor should never be set with a transparent color.");
				Debug.Assert (value == null || MaterialPalettes.Where (p => p.IsAccent).SelectMany (p => p.Colors).Contains (value.Value), "AccentColor values should exist in the Material Design palette.");
				this.accentColor = value;
				if (ColorName != null && value.HasValue) {
					ReplaceColor (value.Value);
				}
				if (value.HasValue) {
					this.normalColor = null;
					OnPropertyChanged (nameof (NormalColor));
				}
				OnPropertyChanged ();
			}
		}

		public IEnumerable<MaterialColorScale> Palettes => MaterialPalettes
			.Where (palette => !palette.IsAccent);
		public IEnumerable<CommonColor?> AccentColorScale => FindPalette(ColorName, true).Colors
			.Select((color, index) => new CommonColor?(new CommonColor(color.R, color.G, color.B, 255, AccentNames[index])));
		public IEnumerable<CommonColor?> NormalColorScale => FindPalette(ColorName, false).Colors
			.Select ((color, index) => new CommonColor? (new CommonColor (color.R, color.G, color.B, 255, NormalNames[index])));

		private string colorName;
		private CommonColor? normalColor = null;
		private CommonColor? accentColor = null;
		private BrushPropertyViewModel Parent { get; }

		private void Parent_PropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof (BrushPropertyViewModel.Value)) {
				OnPropertyChanged (nameof (Color));
				OnPropertyChanged (nameof (Alpha));
				this.colorName = null;
				this.normalColor = null;
				this.accentColor = null;
				OnPropertyChanged (nameof (ColorName));
				OnPropertyChanged (nameof (AccentColor));
				OnPropertyChanged (nameof (NormalColor));
				OnPropertyChanged (nameof (Scale));
				OnPropertyChanged (nameof (AccentColorScale));
				OnPropertyChanged (nameof (NormalColorScale));
			}
		}

		/// <summary>
		/// Replaces the current color with a new one, keeping the alpha channel intact.
		/// </summary>
		private void ReplaceColor(CommonColor newColor)
		{
			var alpha = Color.A;
			Color = new CommonColor (newColor.R, newColor.G, newColor.B, alpha);
		}

		private static string[] NormalNames = new[] { "50", "100", "200", "300", "400", "500", "600", "700", "800", "900" };
		private static string[] AccentNames = new[] { "A100", "A200", "A400", "A700" };

		private static MaterialColorScale FindPalette (string colorName, bool isAccent = false)
		{
			return MaterialPalettes.FirstOrDefault (palette => palette.IsAccent == isAccent && palette.Name == colorName);
		}

		internal static readonly MaterialColorScale[] MaterialPalettes = new [] {
			new MaterialColorScale (Strings.MaterialColorRed, false, 4, new CommonColor(0xFF, 0xEB, 0xEE), new CommonColor(0xFF, 0xCD, 0xD2), new CommonColor(0xEF, 0x9A, 0x9A), new CommonColor(0xE5, 0x73, 0x73), new CommonColor(0xEF, 0x53, 0x50), new CommonColor(0xF4, 0x43, 0x36), new CommonColor(0xE5, 0x39, 0x35), new CommonColor(0xD3, 0x2F, 0x2F), new CommonColor(0xC6, 0x28, 0x28), new CommonColor(0xB7, 0x1C, 0x1C)),

			new MaterialColorScale (Strings.MaterialColorRed, true, 1, new CommonColor(0xFF, 0x8A, 0x80), new CommonColor(0xFF, 0x52, 0x52), new CommonColor(0xFF, 0x17, 0x44), new CommonColor(0xD5, 0x00, 0x00)),

			new MaterialColorScale (Strings.MaterialColorPink, false, 3, new CommonColor(0xFC, 0xE4, 0xEC), new CommonColor(0xF8, 0xBB, 0xD0), new CommonColor(0xF4, 0x8F, 0xB1), new CommonColor(0xF0, 0x62, 0x92), new CommonColor(0xEC, 0x40, 0x7A), new CommonColor(0xE9, 0x1E, 0x63), new CommonColor(0xD8, 0x1B, 0x60), new CommonColor(0xC2, 0x18, 0x5B), new CommonColor(0xAD, 0x14, 0x57), new CommonColor(0x88, 0x0E, 0x4F)),

			new MaterialColorScale (Strings.MaterialColorPink, true, 1, new CommonColor(0xFF, 0x80, 0xAB), new CommonColor(0xFF, 0x40, 0x81), new CommonColor(0xF5, 0x00, 0x57), new CommonColor(0xC5, 0x11, 0x62)),

			new MaterialColorScale (Strings.MaterialColorPurple, false, 3, new CommonColor(0xF3, 0xE5, 0xF5), new CommonColor(0xE1, 0xBE, 0xE7), new CommonColor(0xCE, 0x93, 0xD8), new CommonColor(0xBA, 0x68, 0xC8), new CommonColor(0xAB, 0x47, 0xBC), new CommonColor(0x9C, 0x27, 0xB0), new CommonColor(0x8E, 0x24, 0xAA), new CommonColor(0x7B, 0x1F, 0xA2), new CommonColor(0x6A, 0x1B, 0x9A), new CommonColor(0x4A, 0x14, 0x8C)),

			new MaterialColorScale (Strings.MaterialColorPurple, true, 1, new CommonColor(0xEA, 0x80, 0xFC), new CommonColor(0xE0, 0x40, 0xFB), new CommonColor(0xD5, 0x00, 0xF9), new CommonColor(0xAA, 0x00, 0xFF)),

			new MaterialColorScale (Strings.MaterialColorDeepPurple, false, 3, new CommonColor(0xED, 0xE7, 0xF6), new CommonColor(0xD1, 0xC4, 0xE9), new CommonColor(0xB3, 0x9D, 0xDB), new CommonColor(0x95, 0x75, 0xCD), new CommonColor(0x7E, 0x57, 0xC2), new CommonColor(0x67, 0x3A, 0xB7), new CommonColor(0x5E, 0x35, 0xB1), new CommonColor(0x51, 0x2D, 0xA8), new CommonColor(0x45, 0x27, 0xA0), new CommonColor(0x31, 0x1B, 0x92)),

			new MaterialColorScale (Strings.MaterialColorDeepPurple, true, 1, new CommonColor(0xB3, 0x88, 0xFF), new CommonColor(0x7C, 0x4D, 0xFF), new CommonColor(0x65, 0x1F, 0xFF), new CommonColor(0x62, 0x00, 0xEA)),

			new MaterialColorScale (Strings.MaterialColorIndigo, false, 3, new CommonColor(0xE8, 0xEA, 0xF6), new CommonColor(0xC5, 0xCA, 0xE9), new CommonColor(0x9F, 0xA8, 0xDA), new CommonColor(0x79, 0x86, 0xCB), new CommonColor(0x5C, 0x6B, 0xC0), new CommonColor(0x3F, 0x51, 0xB5), new CommonColor(0x39, 0x49, 0xAB), new CommonColor(0x30, 0x3F, 0x9F), new CommonColor(0x28, 0x35, 0x93), new CommonColor(0x1A, 0x23, 0x7E)),

			new MaterialColorScale (Strings.MaterialColorIndigo, true, 1, new CommonColor(0x8C, 0x9E, 0xFF), new CommonColor(0x53, 0x6D, 0xFE), new CommonColor(0x3D, 0x5A, 0xFE), new CommonColor(0x30, 0x4F, 0xFE)),

			new MaterialColorScale (Strings.MaterialColorBlue, false, 5, new CommonColor(0xE3, 0xF2, 0xFD), new CommonColor(0xBB, 0xDE, 0xFB), new CommonColor(0x90, 0xCA, 0xF9), new CommonColor(0x64, 0xB5, 0xF6), new CommonColor(0x42, 0xA5, 0xF5), new CommonColor(0x21, 0x96, 0xF3), new CommonColor(0x1E, 0x88, 0xE5), new CommonColor(0x19, 0x76, 0xD2), new CommonColor(0x15, 0x65, 0xC0), new CommonColor(0x0D, 0x47, 0xA1)),

			new MaterialColorScale (Strings.MaterialColorBlue, true, 1, new CommonColor(0x82, 0xB1, 0xFF), new CommonColor(0x44, 0x8A, 0xFF), new CommonColor(0x29, 0x79, 0xFF), new CommonColor(0x29, 0x62, 0xFF)),

			new MaterialColorScale (Strings.MaterialColorLightBlue, false, 6, new CommonColor(0xE1, 0xF5, 0xFE), new CommonColor(0xB3, 0xE5, 0xFC), new CommonColor(0x81, 0xD4, 0xFA), new CommonColor(0x4F, 0xC3, 0xF7), new CommonColor(0x29, 0xB6, 0xF6), new CommonColor(0x03, 0xA9, 0xF4), new CommonColor(0x03, 0x9B, 0xE5), new CommonColor(0x02, 0x88, 0xD1), new CommonColor(0x02, 0x77, 0xBD), new CommonColor(0x01, 0x57, 0x9B)),

			new MaterialColorScale (Strings.MaterialColorLightBlue, true, 3, new CommonColor(0x80, 0xD8, 0xFF), new CommonColor(0x40, 0xC4, 0xFF), new CommonColor(0x00, 0xB0, 0xFF), new CommonColor(0x00, 0x91, 0xEA)),

			new MaterialColorScale (Strings.MaterialColorCyan, false, 7, new CommonColor(0xE0, 0xF7, 0xFA), new CommonColor(0xB2, 0xEB, 0xF2), new CommonColor(0x80, 0xDE, 0xEA), new CommonColor(0x4D, 0xD0, 0xE1), new CommonColor(0x26, 0xC6, 0xDA), new CommonColor(0x00, 0xBC, 0xD4), new CommonColor(0x00, 0xAC, 0xC1), new CommonColor(0x00, 0x97, 0xA7), new CommonColor(0x00, 0x83, 0x8F), new CommonColor(0x00, 0x60, 0x64)),

			new MaterialColorScale (Strings.MaterialColorCyan, true, 4, new CommonColor(0x84, 0xFF, 0xFF), new CommonColor(0x18, 0xFF, 0xFF), new CommonColor(0x00, 0xE5, 0xFF), new CommonColor(0x00, 0xB8, 0xD4)),

			new MaterialColorScale (Strings.MaterialColorTeal, false, 5, new CommonColor(0xE0, 0xF2, 0xF1), new CommonColor(0xB2, 0xDF, 0xDB), new CommonColor(0x80, 0xCB, 0xC4), new CommonColor(0x4D, 0xB6, 0xAC), new CommonColor(0x26, 0xA6, 0x9A), new CommonColor(0x00, 0x96, 0x88), new CommonColor(0x00, 0x89, 0x7B), new CommonColor(0x00, 0x79, 0x6B), new CommonColor(0x00, 0x69, 0x5C), new CommonColor(0x00, 0x4D, 0x40)),

			new MaterialColorScale (Strings.MaterialColorTeal, true, 4, new CommonColor(0xA7, 0xFF, 0xEB), new CommonColor(0x64, 0xFF, 0xDA), new CommonColor(0x1D, 0xE9, 0xB6), new CommonColor(0x00, 0xBF, 0xA5)),

			new MaterialColorScale (Strings.MaterialColorGreen, false, 6, new CommonColor(0xE8, 0xF5, 0xE9), new CommonColor(0xC8, 0xE6, 0xC9), new CommonColor(0xA5, 0xD6, 0xA7), new CommonColor(0x81, 0xC7, 0x84), new CommonColor(0x66, 0xBB, 0x6A), new CommonColor(0x4C, 0xAF, 0x50), new CommonColor(0x43, 0xA0, 0x47), new CommonColor(0x38, 0x8E, 0x3C), new CommonColor(0x2E, 0x7D, 0x32), new CommonColor(0x1B, 0x5E, 0x20)),

			new MaterialColorScale (Strings.MaterialColorGreen, true, 4, new CommonColor(0xB9, 0xF6, 0xCA), new CommonColor(0x69, 0xF0, 0xAE), new CommonColor(0x00, 0xE6, 0x76), new CommonColor(0x00, 0xC8, 0x53)),

			new MaterialColorScale (Strings.MaterialColorLightGreen, false, 7, new CommonColor(0xF1, 0xF8, 0xE9), new CommonColor(0xDC, 0xED, 0xC8), new CommonColor(0xC5, 0xE1, 0xA5), new CommonColor(0xAE, 0xD5, 0x81), new CommonColor(0x9C, 0xCC, 0x65), new CommonColor(0x8B, 0xC3, 0x4A), new CommonColor(0x7C, 0xB3, 0x42), new CommonColor(0x68, 0x9F, 0x38), new CommonColor(0x55, 0x8B, 0x2F), new CommonColor(0x33, 0x69, 0x1E)),

			new MaterialColorScale (Strings.MaterialColorLightGreen, true, 4, new CommonColor(0xCC, 0xFF, 0x90), new CommonColor(0xB2, 0xFF, 0x59), new CommonColor(0x76, 0xFF, 0x03), new CommonColor(0x64, 0xDD, 0x17)),

			new MaterialColorScale (Strings.MaterialColorLime, false, 9, new CommonColor(0xF9, 0xFB, 0xE7), new CommonColor(0xF0, 0xF4, 0xC3), new CommonColor(0xE6, 0xEE, 0x9C), new CommonColor(0xDC, 0xE7, 0x75), new CommonColor(0xD4, 0xE1, 0x57), new CommonColor(0xCD, 0xDC, 0x39), new CommonColor(0xC0, 0xCA, 0x33), new CommonColor(0xAF, 0xB4, 0x2B), new CommonColor(0x9E, 0x9D, 0x24), new CommonColor(0x82, 0x77, 0x17)),

			new MaterialColorScale (Strings.MaterialColorLime, true, 4, new CommonColor(0xF4, 0xFF, 0x81), new CommonColor(0xEE, 0xFF, 0x41), new CommonColor(0xC6, 0xFF, 0x00), new CommonColor(0xAE, 0xEA, 0x00)),

			new MaterialColorScale (Strings.MaterialColorYellow, false, 10, new CommonColor(0xFF, 0xFD, 0xE7), new CommonColor(0xFF, 0xF9, 0xC4), new CommonColor(0xFF, 0xF5, 0x9D), new CommonColor(0xFF, 0xF1, 0x76), new CommonColor(0xFF, 0xEE, 0x58), new CommonColor(0xFF, 0xEB, 0x3B), new CommonColor(0xFD, 0xD8, 0x35), new CommonColor(0xFB, 0xC0, 0x2D), new CommonColor(0xF9, 0xA8, 0x25), new CommonColor(0xF5, 0x7F, 0x17)),

			new MaterialColorScale (Strings.MaterialColorYellow, true, 4, new CommonColor(0xFF, 0xFF, 0x8D), new CommonColor(0xFF, 0xFF, 0x00), new CommonColor(0xFF, 0xEA, 0x00), new CommonColor(0xFF, 0xD6, 0x00)),

			new MaterialColorScale (Strings.MaterialColorAmber, false, 10, new CommonColor(0xFF, 0xF8, 0xE1), new CommonColor(0xFF, 0xEC, 0xB3), new CommonColor(0xFF, 0xE0, 0x82), new CommonColor(0xFF, 0xD5, 0x4F), new CommonColor(0xFF, 0xCA, 0x28), new CommonColor(0xFF, 0xC1, 0x07), new CommonColor(0xFF, 0xB3, 0x00), new CommonColor(0xFF, 0xA0, 0x00), new CommonColor(0xFF, 0x8F, 0x00), new CommonColor(0xFF, 0x6F, 0x00)),

			new MaterialColorScale (Strings.MaterialColorAmber, true, 4, new CommonColor(0xFF, 0xE5, 0x7F), new CommonColor(0xFF, 0xD7, 0x40), new CommonColor(0xFF, 0xC4, 0x00), new CommonColor(0xFF, 0xAB, 0x00)),

			new MaterialColorScale (Strings.MaterialColorOrange, false, 8, new CommonColor(0xFF, 0xF3, 0xE0), new CommonColor(0xFF, 0xE0, 0xB2), new CommonColor(0xFF, 0xCC, 0x80), new CommonColor(0xFF, 0xB7, 0x4D), new CommonColor(0xFF, 0xA7, 0x26), new CommonColor(0xFF, 0x98, 0x00), new CommonColor(0xFB, 0x8C, 0x00), new CommonColor(0xF5, 0x7C, 0x00), new CommonColor(0xEF, 0x6C, 0x00), new CommonColor(0xE6, 0x51, 0x00)),

			new MaterialColorScale (Strings.MaterialColorOrange, true, 4, new CommonColor(0xFF, 0xD1, 0x80), new CommonColor(0xFF, 0xAB, 0x40), new CommonColor(0xFF, 0x91, 0x00), new CommonColor(0xFF, 0x6D, 0x00)),

			new MaterialColorScale (Strings.MaterialColorDeepOrange, false, 5, new CommonColor(0xFB, 0xE9, 0xE7), new CommonColor(0xFF, 0xCC, 0xBC), new CommonColor(0xFF, 0xAB, 0x91), new CommonColor(0xFF, 0x8A, 0x65), new CommonColor(0xFF, 0x70, 0x43), new CommonColor(0xFF, 0x57, 0x22), new CommonColor(0xF4, 0x51, 0x1E), new CommonColor(0xE6, 0x4A, 0x19), new CommonColor(0xD8, 0x43, 0x15), new CommonColor(0xBF, 0x36, 0x0C)),

			new MaterialColorScale (Strings.MaterialColorDeepOrange, true, 2, new CommonColor(0xFF, 0x9E, 0x80), new CommonColor(0xFF, 0x6E, 0x40), new CommonColor(0xFF, 0x3D, 0x00), new CommonColor(0xDD, 0x2C, 0x00)),

			new MaterialColorScale (Strings.MaterialColorBrown, false, 3, new CommonColor(0xEF, 0xEB, 0xE9), new CommonColor(0xD7, 0xCC, 0xC8), new CommonColor(0xBC, 0xAA, 0xA4), new CommonColor(0xA1, 0x88, 0x7F), new CommonColor(0x8D, 0x6E, 0x63), new CommonColor(0x79, 0x55, 0x48), new CommonColor(0x6D, 0x4C, 0x41), new CommonColor(0x5D, 0x40, 0x37), new CommonColor(0x4E, 0x34, 0x2E), new CommonColor(0x3E, 0x27, 0x23)),

			new MaterialColorScale (Strings.MaterialColorGrey, false, 6, new CommonColor(0xFA, 0xFA, 0xFA), new CommonColor(0xF5, 0xF5, 0xF5), new CommonColor(0xEE, 0xEE, 0xEE), new CommonColor(0xE0, 0xE0, 0xE0), new CommonColor(0xBD, 0xBD, 0xBD), new CommonColor(0x9E, 0x9E, 0x9E), new CommonColor(0x75, 0x75, 0x75), new CommonColor(0x61, 0x61, 0x61), new CommonColor(0x42, 0x42, 0x42), new CommonColor(0x21, 0x21, 0x21)),

			new MaterialColorScale (Strings.MaterialColorBlueGrey, false, 4, new CommonColor(0xEC, 0xEF, 0xF1), new CommonColor(0xCF, 0xD8, 0xDC), new CommonColor(0xB0, 0xBE, 0xC5), new CommonColor(0x90, 0xA4, 0xAE), new CommonColor(0x78, 0x90, 0x9C), new CommonColor(0x60, 0x7D, 0x8B), new CommonColor(0x54, 0x6E, 0x7A), new CommonColor(0x45, 0x5A, 0x64), new CommonColor(0x37, 0x47, 0x4F), new CommonColor(0x26, 0x32, 0x38)),
			};
	}
}
