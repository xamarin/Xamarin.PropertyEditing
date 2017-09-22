using System.Windows;
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
			this.panel.EditorProvider = new MockEditorProvider();
		}

		private void Button_Click (object sender, RoutedEventArgs e)
		{
			var mockedButton = sender as MockedWpfButton;
			var inspectedObject = (mockedButton == null || mockedButton.MockedControl == null)
				? sender : mockedButton.MockedControl;
			if (this.panel.SelectedItems.Contains (inspectedObject))
				this.panel.SelectedItems.Remove (inspectedObject);
			else
				this.panel.SelectedItems.Add (inspectedObject);
		}
	}
}
