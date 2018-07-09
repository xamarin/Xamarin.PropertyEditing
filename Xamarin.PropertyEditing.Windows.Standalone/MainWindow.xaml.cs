using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
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
			this.panel.TargetPlatform = new TargetPlatform (new MockEditorProvider(), new MockResourceProvider(), new MockBindingProvider()) {
				SupportsCustomExpressions = true,
				SupportsMaterialDesign = true,
				SupportsBrushOpacity = false,
				GroupedTypes = new Dictionary<Type, string> {
					{ typeof(CommonBrush), "Brush" }
				}
			};

			this.panel.ResourceProvider = new MockResourceProvider ();
#if USE_VS_ICONS
			this.panel.Resources.MergedDictionaries.Add (new ResourceDictionary {
				Source = new Uri ("pack://application:,,,/ProppyIcons.xaml", UriKind.RelativeOrAbsolute)
			});
#endif
		}

		private async void Button_Click (object sender, RoutedEventArgs e)
		{
			object inspectedObject;
			if (!(sender is IMockedControl mockedControl) || mockedControl.MockedControl == null) {
				inspectedObject = sender;
			} else {
				inspectedObject = mockedControl.MockedControl;
				if (mockedControl is MockedSampleControlButton mockedButton) {
					IObjectEditor editor = await this.panel.TargetPlatform.EditorProvider.GetObjectEditorAsync (inspectedObject);
					await mockedButton.SetBrushInitialValueAsync (editor, new CommonSolidBrush (20, 120, 220, 240, "sRGB"));
					await mockedButton.SetMaterialDesignBrushInitialValueAsync (editor, new CommonSolidBrush (0x65, 0x1F, 0xFF, 200));
					await mockedButton.SetReadOnlyBrushInitialValueAsync (editor, new CommonSolidBrush (240, 220, 15, 190));
				}
			}

			if (this.panel.SelectedItems.Contains (inspectedObject))
				this.panel.SelectedItems.Remove (inspectedObject);
			else
				this.panel.SelectedItems.Add (inspectedObject);
		}

		private void Theme_Click (object sender, RoutedEventArgs e)
		{
			if (e.Source is RadioButton rb) {
				switch (rb.Content.ToString()) {
				case "Dark Theme":
				PropertyEditorPanel.ThemeManager.Theme = PropertyEditing.Themes.PropertyEditorTheme.Dark;
					break;
				case "Light Theme":
					PropertyEditorPanel.ThemeManager.Theme = PropertyEditing.Themes.PropertyEditorTheme.Light;
					break;
				default:
					PropertyEditorPanel.ThemeManager.Theme = PropertyEditing.Themes.PropertyEditorTheme.None;
					break;
				}
			}
		}
	}
}
