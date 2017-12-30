using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Windows;
using System.Windows.Baml2006;
using System.Xaml;

namespace Xamarin.PropertyEditing.Themes
{

	public class WinThemeManager : BaseThemeManager
	{
		protected override void SetTheme ()
		{
			switch (Theme) {
				case PropertyEditorTheme.Dark:
					Application.Current.Resources.MergedDictionaries.Remove (this.light);
					Application.Current.Resources.MergedDictionaries.Add (this.dark);
					break;

				case PropertyEditorTheme.Light:
					Application.Current.Resources.MergedDictionaries.Remove (this.dark);
					Application.Current.Resources.MergedDictionaries.Add (this.light);
					break;
			}
		}

		protected override string IconModifier => Theme == PropertyEditorTheme.Dark ? "~dark" : "";

		ResourceDictionary dark = new ResourceDictionary () {
			Source = new Uri ("pack://application:,,,/Xamarin.PropertyEditing.Windows;component/Themes/VS.Dark.xaml")
		};
		ResourceDictionary light = new ResourceDictionary () {
			Source = new Uri ("pack://application:,,,/Xamarin.PropertyEditing.Windows;component/Themes/VS.Light.xaml")
		};
	}
}
