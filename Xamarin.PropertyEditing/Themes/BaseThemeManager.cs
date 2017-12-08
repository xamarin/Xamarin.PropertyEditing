using System;

namespace Xamarin.PropertyEditing.Themes
{
	public abstract class BaseThemeManager
	{
		PropertyEditorTheme theme;

		public BaseThemeManager ()
		{
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
			ThemeChanged?.Invoke (null, EventArgs.Empty);
		}

		public event EventHandler ThemeChanged;
	}
}
