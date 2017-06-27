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
	[TemplatePart (Name = "search", Type = typeof(TextBox))]
	[TemplatePart (Name = "propertyItems", Type = typeof(ItemsControl))]
	public class PropertyEditorPanel
		: Control
	{
		static PropertyEditorPanel ()
		{
			// Add Windows specific types
			PanelViewModel.ViewModelMap.Add (typeof (Point), (p, e) => new PropertyViewModel<Point> (p, e));
			PanelViewModel.ViewModelMap.Add (typeof (Size), (p, e) => new PropertyViewModel<Size> (p, e));
			PanelViewModel.ViewModelMap.Add (typeof (Thickness), (p, e) => new PropertyViewModel<Thickness> (p, e));
		}

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

		public static readonly DependencyProperty IsArrangeEnabledProperty = DependencyProperty.Register (
			"IsArrangeEnabled", typeof(bool), typeof(PropertyEditorPanel), new PropertyMetadata (true));

		public bool IsArrangeEnabled
		{
			get { return (bool) GetValue (IsArrangeEnabledProperty); }
			set { SetValue (IsArrangeEnabledProperty, value); }
		}

		public override void OnApplyTemplate ()
		{
			base.OnApplyTemplate ();

			this.root = (FrameworkElement) GetTemplateChild ("root");
			OnEditorProviderChanged();
		}

		private FrameworkElement root;
		private PanelViewModel vm;

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
			if (this.root == null)
				return;

			this.root.DataContext = this.vm = (EditorProvider != null) ? new PanelViewModel (EditorProvider) : null;
		}
	}
}