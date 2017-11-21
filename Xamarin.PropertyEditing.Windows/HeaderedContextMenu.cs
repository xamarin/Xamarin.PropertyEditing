using System.Windows;
using System.Windows.Controls;

namespace Xamarin.PropertyEditing.Windows
{
	internal class HeaderedContextMenu
		: ContextMenu
	{
		public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register (
			"Header", typeof (object), typeof (HeaderedContextMenu), new PropertyMetadata (default (object)));

		public object Header
		{
			get { return (object)GetValue (HeaderProperty); }
			set { SetValue (HeaderProperty, value); }
		}

		public static readonly DependencyProperty HeaderTemplateProperty = DependencyProperty.Register (
			"HeaderTemplate", typeof (DataTemplate), typeof (HeaderedContextMenu), new PropertyMetadata (default (DataTemplate)));

		public DataTemplate HeaderTemplate
		{
			get { return (DataTemplate)GetValue (HeaderTemplateProperty); }
			set { SetValue (HeaderTemplateProperty, value); }
		}
	}
}