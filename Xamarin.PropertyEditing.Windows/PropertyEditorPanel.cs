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

	[TemplatePart (Name = "search", Type = typeof(TextBox))]
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

			// Add Windows specific types
			PanelViewModel.ViewModelMap.Add (typeof (Point), (p, e) => new PropertyViewModel<Point> (p, e));
			PanelViewModel.ViewModelMap.Add (typeof (Size), (p, e) => new PropertyViewModel<Size> (p, e));
			PanelViewModel.ViewModelMap.Add (typeof (Thickness), (p, e) => new PropertyViewModel<Thickness> (p, e));
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

			this.search = (TextBox) GetTemplateChild ("search");
			if (this.search == null)
				throw new InvalidOperationException ("PropertyEditorPanel template missing part search");

			this.search.TextChanged += (sender, args) => {
				if (this.items?.ItemsSource == null)
					return;

				ICollectionView view = CollectionViewSource.GetDefaultView (this.items.ItemsSource);
				view.Refresh();
			};

			this.items = (ItemsControl)GetTemplateChild ("propertyItems");
			if (this.items == null)
				throw new InvalidOperationException ("PropertyEditorPanel template missing part propertyItems");

			DependencyPropertyDescriptor.FromProperty (ItemsControl.ItemsSourceProperty, typeof (ItemsControl))
				.AddValueChanged (this.items, OnItemsSourceChanged);

			OnEditorProviderChanged();
		}

		private PanelViewModel vm;
		private ItemsControl items;
		private TextBox search;

		private void OnItemsSourceChanged (object sender, EventArgs eventArgs)
		{
			if (this.items?.ItemsSource == null)
				return;

			ICollectionView view = CollectionViewSource.GetDefaultView (this.items.ItemsSource);
			view.Filter = o => {
				string f = this.search.Text;
				if (String.IsNullOrWhiteSpace (f))
					return true;

				PropertyViewModel pvm = o as PropertyViewModel;
				if (pvm == null)
					return true;

				return pvm.Property.Name.StartsWith (f, StringComparison.InvariantCultureIgnoreCase);
			};

			OnGroupModeChanged ();
		}

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

			switch (e.Action) {
				case NotifyCollectionChangedAction.Add:
					for (int i = 0; i < e.NewItems.Count; i++)
						this.vm.SelectedObjects.Add (e.NewItems[i]);
					break;

				case NotifyCollectionChangedAction.Remove:
					for (int i = 0; i < e.OldItems.Count; i++)
						this.vm.SelectedObjects.Remove (e.OldItems[i]);
					break;

				case NotifyCollectionChangedAction.Replace: // TODO properly
				case NotifyCollectionChangedAction.Move:
				case NotifyCollectionChangedAction.Reset:
					this.vm.SelectedObjects.Clear();
					this.vm.SelectedObjects.AddItems (SelectedItems);
					break;
			}
		}

		private void OnEditorProviderChanged ()
		{
			if (this.items == null)
				return;

			this.items.DataContext = this.vm = (EditorProvider != null) ? new PanelViewModel (EditorProvider) : null;
		}
	}
}