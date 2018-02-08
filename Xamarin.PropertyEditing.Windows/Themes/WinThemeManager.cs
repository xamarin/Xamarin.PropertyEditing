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
		ResourceDictionary dark = new ResourceDictionary () {
			Source = new Uri ("pack://application:,,,/Xamarin.PropertyEditing.Windows;component/Themes/VS.Dark.xaml")
		};
		ResourceDictionary light = new ResourceDictionary () {
			Source = new Uri ("pack://application:,,,/Xamarin.PropertyEditing.Windows;component/Themes/VS.Light.xaml")
		};

		protected override void SetTheme ()
		{
			switch (Theme) {
				case PropertyEditorTheme.Dark:
					Application.Current.Resources.MergedDictionaries.Remove (light);
					Application.Current.Resources.MergedDictionaries.Add (dark);
					break;

				case PropertyEditorTheme.Light:
					Application.Current.Resources.MergedDictionaries.Remove (dark);
					Application.Current.Resources.MergedDictionaries.Add (light);
					break;

				case PropertyEditorTheme.None:
					Application.Current.Resources.MergedDictionaries.Remove (dark);
					Application.Current.Resources.MergedDictionaries.Remove (light);
					break;
			}
		}
	}
}
