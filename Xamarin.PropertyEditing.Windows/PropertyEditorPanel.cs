using System.Collections;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Windows
{
	[TemplatePart (Name = "propertyItems", Type = typeof(ItemsControl))]
	public class PropertyEditorPanel
		: Control
	{
		public PropertyEditorPanel ()
		{
			DefaultStyleKey = typeof(PropertyEditorPanel);
			SelectedItems = new ObservableCollection<object> ();
		}

		public static readonly DependencyProperty EditorProviderProperty = DependencyProperty.Register (
			"EditorProvider", typeof(IEditorProvider), typeof(PropertyEditorPanel), new PropertyMetadata (default(IEditorProvider)));

		public IEditorProvider EditorProvider
		{
			get { return (IEditorProvider) GetValue (EditorProviderProperty); }
			set { SetValue (EditorProviderProperty, value); }
		}

		private static readonly DependencyPropertyKey SelectedItemsPropertyKey = DependencyProperty.RegisterReadOnly (
			"SelectedItems", typeof(IList), typeof(PropertyEditorPanel), new PropertyMetadata (default(IList)));

		public static readonly DependencyProperty SelectedItemsProperty = SelectedItemsPropertyKey.DependencyProperty;

		public IList SelectedItems
		{
			get { return (IList) GetValue (SelectedItemsProperty); }
			private set { SetValue (SelectedItemsPropertyKey, value); }
		}

		public override void OnApplyTemplate ()
		{
			base.OnApplyTemplate ();

			this.items = (ItemsControl)GetTemplateChild ("propertyItems");
			this.items.DataContext = new PanelViewModel (EditorProvider);
		}

		private ItemsControl items;

		private void OnEditorChanged ()
		{
			
		}
	}
}