using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Windows
{
	public enum PropertyArrangeMode
	{
		Name = 0,
		Category = 1,
		ValueSource = 2
	}

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

		public static readonly DependencyProperty ArrangeModeProperty = DependencyProperty.Register (
			"ArrangeMode", typeof(PropertyArrangeMode), typeof(PropertyEditorPanel), new PropertyMetadata (PropertyArrangeMode.Name, (o, args) => ((PropertyEditorPanel)o).OnGroupModeChanged ()));

		public PropertyArrangeMode ArrangeMode
		{
			get { return (PropertyArrangeMode) GetValue (ArrangeModeProperty); }
			set { SetValue (ArrangeModeProperty, value); }
		}

		public override void OnApplyTemplate ()
		{
			base.OnApplyTemplate ();

			this.items = (ItemsControl)GetTemplateChild ("propertyItems");
			this.items.DataContext = this.vm = new PanelViewModel (EditorProvider);
			Dispatcher.InvokeAsync (OnGroupModeChanged); // trigger after binding finishes
		}

		private PanelViewModel vm;
		private ItemsControl items;

		private void OnGroupModeChanged ()
		{
			if (this.items?.ItemsSource == null)
				return;

			PropertyArrangeMode mode = ArrangeMode;

			ICollectionView view = CollectionViewSource.GetDefaultView (this.items.ItemsSource);

			using (view.DeferRefresh ()) {
				view.GroupDescriptions.Clear ();
				view.SortDescriptions.Clear ();

				switch (mode) {
					case PropertyArrangeMode.Name:
						view.SortDescriptions.Add (new SortDescription ("Property.Name", ListSortDirection.Ascending));
						break;
					case PropertyArrangeMode.Category:
						view.GroupDescriptions.Add (new PropertyGroupDescription ("Category"));
						view.SortDescriptions.Add (new SortDescription ("Category", ListSortDirection.Ascending));
						view.SortDescriptions.Add (new SortDescription ("Property.Name", ListSortDirection.Ascending));
						break;
				}
			}
		}

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