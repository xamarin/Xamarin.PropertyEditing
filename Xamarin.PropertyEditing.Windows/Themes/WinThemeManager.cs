using System;
using System.Drawing;

namespace Xamarin.PropertyEditing.Themes
{

	public class WinThemeManager : BaseThemeManager
	{
		protected override void SetStyles ()
		{
			switch (Theme) {
				case PropertyEditorTheme.Dark:
					Style.BackgroundColor = Color.FromArgb (255, 38, 38, 38);
					Style.CellBackgroundColor = Color.FromArgb (255, 38, 38, 38);
					Style.FontSize = 11;
					Style.ItemSelectionColor = Color.FromArgb (255, 128, 128, 128);
					Style.TextColor = Color.FromArgb (255, 213, 213, 213);
					break;

				case PropertyEditorTheme.Light:
					Style.BackgroundColor = Color.FromArgb (255, 240, 241, 243);
					Style.CellBackgroundColor = Color.FromArgb (255, 240, 241, 243);
					Style.FontSize = 11;
					Style.ItemSelectionColor = Color.FromArgb (255, 0, 0, 245);
					Style.TextColor = Color.FromArgb (255, 68, 68, 68);
					break;
			}
		}
	}
}
