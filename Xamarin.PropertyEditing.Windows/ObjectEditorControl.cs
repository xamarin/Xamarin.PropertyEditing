using System.Windows;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Windows
{
    internal class ObjectEditorControl
		: PropertyEditorControl
    {
		static ObjectEditorControl()
		{
			DefaultStyleKeyProperty.OverrideMetadata (typeof(ObjectEditorControl), new FrameworkPropertyMetadata (typeof(ObjectEditorControl)));
		}

		public ObjectEditorControl()
		{
			DataContextChanged += OnDataContextChanged;
		}

		private void OnDataContextChanged (object sender, DependencyPropertyChangedEventArgs e)
		{
			var vm = e.OldValue as ObjectPropertyViewModel;
			if (vm != null)
				vm.TypeRequested -= OnTypeRequested;

			vm = e.NewValue as ObjectPropertyViewModel;
			if (vm != null)
				vm.TypeRequested += OnTypeRequested;
		}

		private void OnTypeRequested (object sender, TypeRequestedEventArgs e)
		{
			var vm = (ObjectPropertyViewModel)sender;

			var panel = this.FindPropertiesHost ();

			ITypeInfo type = TypeSelectorWindow.RequestType (panel, vm.AssignableTypes);
			e.SelectedType = type;
		}
	}
}
