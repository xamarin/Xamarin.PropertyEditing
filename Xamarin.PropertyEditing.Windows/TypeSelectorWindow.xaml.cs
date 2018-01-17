using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Windows
{
	internal partial class TypeSelectorWindow
		: WindowEx
	{
		internal TypeSelectorWindow (AsyncValue<IReadOnlyDictionary<IAssemblyInfo, ILookup<string, ITypeInfo>>> assignableTypes)
		{
			InitializeComponent ();
			DataContext = new TypeSelectorViewModel (assignableTypes);
		}

		internal static ITypeInfo RequestType (Window owner, AsyncValue<IReadOnlyDictionary<IAssemblyInfo, ILookup<string, ITypeInfo>>> assignableTypes)
		{
			var w = new TypeSelectorWindow (assignableTypes);
			w.Owner = owner;
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
