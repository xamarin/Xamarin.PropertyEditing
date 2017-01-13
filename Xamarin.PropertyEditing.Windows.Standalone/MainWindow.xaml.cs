using System.Windows;
using Xamarin.PropertyEditing.Reflection;

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
			this.panel.EditorProvider = new ReflectionEditorProvider();
		}

		private void Button_Click (object sender, RoutedEventArgs e)
		{
			if (this.panel.SelectedItems.Contains (sender))
				this.panel.SelectedItems.Remove (sender);
			else
				this.panel.SelectedItems.Add (sender);
		}
	}
}
