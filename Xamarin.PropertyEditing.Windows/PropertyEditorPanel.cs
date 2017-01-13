using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
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

			var selectedItems = new ObservableCollection<object> ();
			selectedItems.CollectionChanged += OnSelectedItemsChanged;
			SelectedItems = selectedItems;
		}

		public static readonly DependencyProperty EditorProviderProperty = DependencyProperty.Register (
			"EditorProvider", typeof(IEditorProvider), typeof(PropertyEditorPanel), new PropertyMetadata (default(IEditorProvider), (o, args) => ((PropertyEditorPanel)o).OnEditorProviderChanged()));

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
			this.items.DataContext = this.vm = new PanelViewModel (EditorProvider);
		}

		private PanelViewModel vm;
		private ItemsControl items;

		private void OnSelectedItemsChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			if (this.vm == null)
				return;

			// TODO properly
			this.vm.SelectedObjects.Clear();
			this.vm.SelectedObjects.AddRange (SelectedItems.Cast<object>());
		}

		private void OnEditorProviderChanged ()
		{
			if (this.items == null)
				return;

			this.items.DataContext = this.vm = (EditorProvider != null) ? new PanelViewModel (EditorProvider) : null;
		}
	}
}