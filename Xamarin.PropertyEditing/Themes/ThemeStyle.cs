using System;
using System.Drawing;

namespace Xamarin.PropertyEditing.Themes
{
	public class ThemeStyle
	{
		// Please try and keep this in alphabetical ;)
		public Color BackgroundColor { get; internal set; }
		public Color CellBackgroundColor { get; internal set; }
		public int FontSize { get; internal set; }
		public Color ItemSelectionColor { get; internal set; }
		public Color TextColor { get; internal set; }
	}

	public enum PropertyEditorTheme
	{
		Dark,
		Light,
	}
}
