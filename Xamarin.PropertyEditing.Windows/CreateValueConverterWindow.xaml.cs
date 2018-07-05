using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Windows
{
    internal partial class CreateValueConverterWindow
	    : WindowEx
    {
	    internal CreateValueConverterWindow (IEnumerable<ResourceDictionary> mergedResources, TargetPlatform platform, object target, AsyncValue<IReadOnlyDictionary<IAssemblyInfo, ILookup<string, ITypeInfo>>> assignableTypes)
	    {
		    DataContext = new AddValueConverterViewModel (platform, target, assignableTypes);
		    InitializeComponent ();
			Resources.MergedDictionaries.AddItems (mergedResources);
	    }

	    internal static Tuple<string, ITypeInfo> RequestConverter (FrameworkElement owner, TargetPlatform platform, object target, AsyncValue<IReadOnlyDictionary<IAssemblyInfo, ILookup<string, ITypeInfo>>> assignableTypes)
	    {
		    Window hostWindow = Window.GetWindow (owner);
		    var w = new CreateValueConverterWindow (owner.Resources.MergedDictionaries, platform, target, assignableTypes) {
			    Owner = hostWindow,
		    };

		    if (!w.ShowDialog () ?? false)
			    return null;

		    return new Tuple<string, ITypeInfo> (w.converterName.Text, w.typeSelector.SelectedItem as ITypeInfo);
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
