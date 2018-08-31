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

		private ObjectPropertyViewModel vm;

		private void OnDataContextChanged (object sender, DependencyPropertyChangedEventArgs e)
		{
			if (this.vm != null)
				this.vm.TypeRequested -= OnTypeRequested;

			this.vm = e.NewValue as ObjectPropertyViewModel;
			if (this.vm != null)
				this.vm.TypeRequested += OnTypeRequested;
		}

		private void OnTypeRequested (object sender, TypeRequestedEventArgs e)
		{
			var vsender = (ObjectPropertyViewModel)sender;

			var panel = this.FindPropertiesHost ();

			ITypeInfo type = TypeSelectorWindow.RequestType (panel, vsender.AssignableTypes);
			e.SelectedType = type;
		}
	}
}
