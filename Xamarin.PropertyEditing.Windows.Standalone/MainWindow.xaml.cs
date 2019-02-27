using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Xamarin.PropertyEditing.Drawing;
using Xamarin.PropertyEditing.Tests;

namespace Xamarin.PropertyEditing.Windows.Standalone
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow ()
		{
			InitializeComponent ();

			this.fonts.ItemsSource = Fonts.SystemFontFamilies;
			this.fonts.SelectedItem = FontFamily;

			this.fontSize.Text = this.fontSizeConverter.ConvertToString (FontSize);

			this.locale.ItemsSource = CultureInfo.GetCultures (CultureTypes.AllCultures);
			this.locale.SelectedItem = CultureInfo.CurrentUICulture;

			var resources = new MockResourceProvider();
			this.panel.TargetPlatform = new TargetPlatform (new MockEditorProvider (resources), resources, new MockBindingProvider()) {
				SupportsCustomExpressions = true,
				SupportsMaterialDesign = true,
				SupportsBrushOpacity = false,
				GroupedTypes = new Dictionary<Type, string> {
					{ typeof(CommonBrush), "Brush" }
				},
				AutoExpandGroups = new[] { "ReadWrite" }
			};

			Application.Current.Resources.MergedDictionaries.Add (DarkTheme);

#if USE_VS_ICONS
			this.panel.Resources.MergedDictionaries.Add (new ResourceDictionary {
				Source = new Uri ("pack://application:,,,/ProppyIcons.xaml", UriKind.RelativeOrAbsolute)
			});
#endif
		}

		private static readonly ResourceDictionary DarkTheme = new ResourceDictionary () {
			Source = new Uri ("pack://application:,,,/Xamarin.PropertyEditing.Windows;component/Themes/VS.Dark.xaml")
		};
		private static readonly ResourceDictionary LightTheme = new ResourceDictionary () {
			Source = new Uri ("pack://application:,,,/Xamarin.PropertyEditing.Windows;component/Themes/VS.Light.xaml")
		};

		private readonly FontSizeConverter fontSizeConverter = new FontSizeConverter();

		private async void Button_Click (object sender, RoutedEventArgs e)
		{
			object inspectedObject;
			if (!(sender is IMockedControl mockedControl) || mockedControl.MockedControl == null) {
				inspectedObject = sender;
			} else {
				inspectedObject = mockedControl.MockedControl;
				if (mockedControl is MockedSampleControlButton mockedButton) {
					IObjectEditor editor = await this.panel.TargetPlatform.EditorProvider.GetObjectEditorAsync (inspectedObject);
					await mockedButton.MockedControl.SetBrushInitialValueAsync (editor, new CommonSolidBrush (20, 120, 220, 240, "sRGB"));
					await mockedButton.MockedControl.SetMaterialDesignBrushInitialValueAsync (editor, new CommonSolidBrush (0x65, 0x1F, 0xFF, 200));
					await mockedButton.MockedControl.SetReadOnlyBrushInitialValueAsync (editor, new CommonSolidBrush (240, 220, 15, 190));
				}
			}

			if (this.panel.SelectedItems.Contains (inspectedObject))
				this.panel.SelectedItems.Remove (inspectedObject);
			else
				this.panel.SelectedItems.Add (inspectedObject);
		}

		private void Fonts_SelectionChanged (object sender, SelectionChangedEventArgs e)
		{
			FontFamily = (FontFamily) this.fonts.SelectedItem;
		}

		private void FontSize_TextChanged (object sender, TextChangedEventArgs e)
		{
			try {
				object size = this.fontSizeConverter.ConvertFromString (this.fontSize.Text);
				if (size == null)
					return;

				FontSize = (double) size;
			} catch (FormatException) {
			} catch (NotSupportedException) {
			}
		}

		private void Locale_SelectionChanged (object sender, SelectionChangedEventArgs e)
		{
			CultureInfo.CurrentUICulture = (CultureInfo) this.locale.SelectedItem;
		}

		private void Theme_SelectionChanged (object sender, SelectionChangedEventArgs e)
		{
			switch ((this.theme.SelectedItem as ComboBoxItem)?.Content?.ToString ()) {
			case "Dark":
				Application.Current.Resources.MergedDictionaries.Remove (LightTheme);
				Application.Current.Resources.MergedDictionaries.Add (DarkTheme);
				break;
			case "Light":
				Application.Current.Resources.MergedDictionaries.Remove (DarkTheme);
				Application.Current.Resources.MergedDictionaries.Add (LightTheme);
				break;
			default:
				Application.Current.Resources.MergedDictionaries.Remove (LightTheme);
				Application.Current.Resources.MergedDictionaries.Remove (DarkTheme);
				break;
			}
		}
	}
}
