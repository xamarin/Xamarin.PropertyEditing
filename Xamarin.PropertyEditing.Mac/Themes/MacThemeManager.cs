using System;
using System.Drawing;
using AppKit;

namespace Xamarin.PropertyEditing.Themes
{
	public class MacThemeManager : BaseThemeManager
	{
		public NSAppearance CurrentAppearance { get; private set; }

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
			}
		}
	
		protected override string IconModifier => Theme == PropertyEditorTheme.Dark ? "~dark" : "";
	}
}
