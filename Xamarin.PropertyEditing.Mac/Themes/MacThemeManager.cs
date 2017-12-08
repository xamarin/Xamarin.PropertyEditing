using System;
using System.Drawing;
using AppKit;

namespace Xamarin.PropertyEditing.Themes
{
	public class MacThemeManager : BaseThemeManager
	{
		public NSAppearance CurrentAppearance { get; private set; }

		NSAppearance DarkAppearance = new NSAppearance ("Themes/PropertyEditorAppearance.Dark", null);
		NSAppearance LightAppearance = new NSAppearance ("Themes/PropertyEditorAppearance.Light", null);

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
	}
}
