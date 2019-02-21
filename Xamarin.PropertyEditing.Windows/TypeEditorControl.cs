using System.Threading.Tasks;
using System.Windows;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Windows
{
	internal class TypeEditorControl
		: PropertyEditorControl
	{
		static TypeEditorControl ()
		{
			DefaultStyleKeyProperty.OverrideMetadata (typeof (TypeEditorControl), new FrameworkPropertyMetadata (typeof (TypeEditorControl)));
		}

		public TypeEditorControl ()
		{
			DataContextChanged += OnDataContextChanged;
		}

		private TypePropertyViewModel vm;

		private void OnDataContextChanged (object sender, DependencyPropertyChangedEventArgs e)
		{
			if (this.vm != null)
				this.vm.TypeRequested -= OnTypeRequested;

			this.vm = e.NewValue as TypePropertyViewModel;
			if (this.vm != null)
				this.vm.TypeRequested += OnTypeRequested;
		}

		private void OnTypeRequested (object sender, TypeRequestedEventArgs e)
		{
			var vsender = (TypePropertyViewModel)sender;

			var panel = this.FindPropertiesHost ();

			ITypeInfo type = TypeSelectorWindow.RequestType (panel, vsender.AssignableTypes);
			e.SelectedType = Task.FromResult (type);
		}
	}
}
