using System.Windows;
using System.Windows.Controls;

namespace Xamarin.PropertyEditing.Windows
{
	public abstract class PropertyEditorControl
		: Control
	{
		static PropertyEditorControl ()
		{
			FocusableProperty.OverrideMetadata (typeof(PropertyEditorControl), new FrameworkPropertyMetadata (false));
		}

		public static readonly DependencyProperty LabelProperty = DependencyProperty.Register (
			nameof(Label), typeof(object), typeof(PropertyEditorControl), new PropertyMetadata (default(object)));

		public object Label
		{
			get { return (object) GetValue (LabelProperty); }
			set { SetValue (LabelProperty, value); }
		}
	}
}