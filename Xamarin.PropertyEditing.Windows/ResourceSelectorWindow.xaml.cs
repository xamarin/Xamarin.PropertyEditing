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
		public ResourceSelectorWindow (IEnumerable<ResourceDictionary> mergedResources, IResourceProvider resourceProvider, IEnumerable<object> targets, IPropertyInfo property)
		{
			Resources.MergedDictionaries.AddItems (mergedResources);
			DataContext = new ResourceSelectorViewModel (resourceProvider, targets, property);
			InitializeComponent ();
		}

		internal static Resource RequestResource (PropertyEditorPanel owner, IResourceProvider provider, IEnumerable<object> targets, IPropertyInfo property, Resource currentValue)
		{
			Window hostWindow = Window.GetWindow (owner);

			var w = new ResourceSelectorWindow (owner.Resources.MergedDictionaries, provider, targets, property) {
				Owner = hostWindow,
				list = {
					SelectedItem = currentValue
				}
			};

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
