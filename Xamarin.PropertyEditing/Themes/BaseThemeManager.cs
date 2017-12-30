using System;

namespace Xamarin.PropertyEditing.Themes
{
	public abstract class BaseThemeManager
	{
		public BaseThemeManager ()
		{
			NotifyThemeChanged ();
		}

		public PropertyEditorTheme Theme
		{
			get {
				return this.theme;
			}

			set {
				if (this.theme != value) {
					this.theme = value;
					NotifyThemeChanged ();
				}
			}
		}

		public string GetIconName (string name, int size, bool selected = false)
			=> $"{name}-{size}{IconModifier}{(selected ? "~sel" : "")}";

		protected abstract void SetTheme ();

		protected abstract string IconModifier { get; }

		PropertyEditorTheme theme;

		void NotifyThemeChanged ()
		{
			SetTheme ();
			ThemeChanged?.Invoke (null, EventArgs.Empty);
		}

		public event EventHandler ThemeChanged;
	}
}
