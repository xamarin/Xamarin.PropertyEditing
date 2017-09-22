using System.Windows;
using System.Windows.Controls;
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
			this.panel.EditorProvider = new MockEditorProvider ();
		}

		private void Button_Click (object sender, RoutedEventArgs e)
		{
			var mockedButton = sender as IMockedControl;
			var inspectedObject = (mockedButton == null || mockedButton.MockedControl == null)
				? sender : mockedButton.MockedControl;
			if (this.panel.SelectedItems.Contains (inspectedObject))
				this.panel.SelectedItems.Remove (inspectedObject);
			else
				this.panel.SelectedItems.Add (inspectedObject);
		}

		private void Theme_Click (object sender, RoutedEventArgs e)
		{
			var rb = e.Source as RadioButton;
			if (rb != null) {
				if (rb.Content.ToString ().Equals ("Dark Theme")) {
					PropertyEditorPanel.ThemeManager.Theme = PropertyEditing.Themes.PropertyEditorTheme.Dark;
				} else {
					PropertyEditorPanel.ThemeManager.Theme = PropertyEditing.Themes.PropertyEditorTheme.Light;
				}
			}
		}
	}
}
