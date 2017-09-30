using System;
using System.Drawing;
using System.Linq;

namespace Xamarin.PropertyEditing.Themes
{
	public abstract class BaseThemeManager
	{
		PropertyEditorTheme theme;

		public ThemeStyle Style { get; private set; }

		public BaseThemeManager ()
		{
			Style = new ThemeStyle ();
			NotifyThemeChanged ();
		}

		protected abstract void SetTheme ();

		public PropertyEditorTheme Theme
		{
			get {
				return theme;
			}

			set {
				if (theme != value) {
					theme = value;
					NotifyThemeChanged ();
				}
			}
		}

		void NotifyThemeChanged ()
		{
			SetTheme ();
			if (ThemeChanged != null)
				ThemeChanged (null, EventArgs.Empty);
		}

		public event EventHandler ThemeChanged;
	}
}
