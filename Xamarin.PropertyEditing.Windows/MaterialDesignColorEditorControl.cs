using System.Windows;

namespace Xamarin.PropertyEditing.Windows
{
	internal class MaterialDesignColorEditorControl
		: PropertyEditorControl
	{
		static MaterialDesignColorEditorControl ()
		{
			FocusableProperty.OverrideMetadata (typeof (MaterialDesignColorEditorControl), new FrameworkPropertyMetadata (false));
			DefaultStyleKeyProperty.OverrideMetadata (typeof (MaterialDesignColorEditorControl), new FrameworkPropertyMetadata (typeof (MaterialDesignColorEditorControl)));
		}
	}
}
