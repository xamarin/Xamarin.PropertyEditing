using System;
using System.Drawing;
using AppKit;

namespace Xamarin.PropertyEditing.Themes
{
	public class MacThemeManager : BaseThemeManager
	{
		public NSAppearance CurrentAppearance { get; private set; }

		NSAppearance DarkAppearance = NSAppearance.GetAppearance (NSAppearance.NameVibrantDark);
		NSAppearance LightAppearance = NSAppearance.GetAppearance (NSAppearance.NameVibrantLight);

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
		}

		public string GetImageNameForTheme (string imageNamed)
		{
			return Theme == PropertyEditorTheme.Dark ? imageNamed + "~dark" : imageNamed;
		}

		public NSImage GetImageForTheme (string imageNamed)
		{
			return NSImage.ImageNamed (GetImageNameForTheme (imageNamed));
		}
	}
}
