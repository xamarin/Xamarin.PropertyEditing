using System.Windows;
using System.Windows.Controls;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Windows
{
	class ResourceBrushEditorControl : PropertyEditorControl
	{
		static ResourceBrushEditorControl ()
		{
			DefaultStyleKeyProperty.OverrideMetadata (typeof (ResourceBrushEditorControl), new FrameworkPropertyMetadata (typeof (ResourceBrushEditorControl)));
		}

		BrushPropertyViewModel ViewModel => DataContext as BrushPropertyViewModel;
	}
}
