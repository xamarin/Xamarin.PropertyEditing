using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Windows
{
	/// <summary>
	/// Interaction logic for ResourceSelectorWindow.xaml
	/// </summary>
	internal partial class ResourceSelectorWindow
		: WindowEx
	{
		public ResourceSelectorWindow (IResourceProvider resourceProvider, IEnumerable<object> targets, IPropertyInfo property)
		{
			DataContext = new ResourceSelectorViewModel (resourceProvider, targets, property);
			InitializeComponent ();
		}

		internal static Resource RequestResource (Window owner, IResourceProvider provider, IEnumerable<object> targets, IPropertyInfo property)
		{
			var w = new ResourceSelectorWindow (provider, targets, property);
			w.Owner = owner;
			if (!w.ShowDialog () ?? false)
				return null;

			return w.list.SelectedItem as Resource;
		}

		private void OnOkClicked (object sender, RoutedEventArgs e)
		{
			DialogResult = true;
		}

		private void OnListSelectionChanged (object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			this.ok.IsEnabled = (e.AddedItems.Count == 1 && e.AddedItems[0] is Resource);
		}

		private void OnListDoubleClick (object sender, MouseButtonEventArgs e)
		{
			if (this.list.SelectedItem == null)
				return;

			DialogResult = true;
		}
	}
}
