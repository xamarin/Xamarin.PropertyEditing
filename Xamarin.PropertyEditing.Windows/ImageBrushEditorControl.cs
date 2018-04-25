using System.Windows;

namespace Xamarin.PropertyEditing.Windows
{
	internal class ImageBrushEditorControl
		: PropertyEditorControl
	{
		static ImageBrushEditorControl ()
		{
			FocusableProperty.OverrideMetadata (typeof (ImageBrushEditorControl), new FrameworkPropertyMetadata (false));
			DefaultStyleKeyProperty.OverrideMetadata (typeof (ImageBrushEditorControl), new FrameworkPropertyMetadata (typeof (ImageBrushEditorControl)));
		}
	}
}
