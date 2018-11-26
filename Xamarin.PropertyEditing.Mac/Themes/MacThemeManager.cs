using System;
using System.Drawing;
using System.Collections.Generic;
using AppKit;

namespace Xamarin.PropertyEditing.Themes
{
	public class MacThemeManager : BaseThemeManager
	{
		public NSAppearance CurrentAppearance { get; private set; }
		private Dictionary<string, NSImage> themeCache = new Dictionary<string, NSImage> ();

		NSAppearance DarkAppearance = NSAppearance.GetAppearance (NSAppearance.NameVibrantDark);
		NSAppearance LightAppearance = NSAppearance.GetAppearance (NSAppearance.NameAqua);

		protected override void SetTheme ()
		{
			switch (Theme) {
				case PropertyEditorTheme.Dark:
					CurrentAppearance = DarkAppearance;
					break;

				case PropertyEditorTheme.Light:
					CurrentAppearance = LightAppearance;
					break;

				case PropertyEditorTheme.None:
					CurrentAppearance = NSAppearance.CurrentAppearance;
					break;
			}

			this.themeCache.Clear ();
		}

		public string GetImageNameForTheme (string imageNamed, bool selected = false)
		{
			return (Theme == PropertyEditorTheme.Dark ? imageNamed + "~dark" : imageNamed) + (selected ? "~sel" : string.Empty);
		}

		public NSImage GetImageForTheme (string imageNamed, bool selected = false)
		{
			var imageNameForTheme = GetImageNameForTheme (imageNamed, selected);

			if (!this.themeCache.TryGetValue (imageNameForTheme, out NSImage themeImage)) {
				themeImage = NSImage.ImageNamed (imageNameForTheme);
				this.themeCache[imageNameForTheme] = themeImage;
			}
			return themeImage;
		}
	}
}
