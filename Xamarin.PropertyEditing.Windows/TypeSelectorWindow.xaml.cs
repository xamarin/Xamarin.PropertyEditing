using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Windows
{
	internal partial class TypeSelectorWindow
		: WindowEx
	{
		internal TypeSelectorWindow (IEnumerable<ResourceDictionary> mergedResources, AsyncValue<IReadOnlyDictionary<IAssemblyInfo, ILookup<string, ITypeInfo>>> assignableTypes)
		{
			Resources.MergedDictionaries.AddItems (mergedResources);
			DataContext = new TypeSelectorViewModel (assignableTypes);
			InitializeComponent ();
		}

		internal static ITypeInfo RequestType (PropertyEditorPanel owner, AsyncValue<IReadOnlyDictionary<IAssemblyInfo, ILookup<string, ITypeInfo>>> assignableTypes)
		{
			Window hostWindow = Window.GetWindow (owner);

			var w = new TypeSelectorWindow (owner.Resources.MergedDictionaries, assignableTypes) {
				Owner = hostWindow,
			};

			w.Resources.MergedDictionaries.AddItems (owner.Resources.MergedDictionaries);

			if (!w.ShowDialog () ?? false)
				return null;

			return w.tree.SelectedItem as ITypeInfo;
		}

		private void OnSelectedItemChanged (object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			this.ok.IsEnabled = (e.NewValue as ITypeInfo) != null;
		}

		private void OnItemActivated (object sender, System.EventArgs e)
		{
			DialogResult = true;
		}

		private void OnOkClicked (object sender, RoutedEventArgs e)
		{
			DialogResult = true;
		}
	}
}
