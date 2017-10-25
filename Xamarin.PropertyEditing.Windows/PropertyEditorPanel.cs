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
	[TemplatePart (Name = "paneSelector", Type = typeof(ChoiceControl))]
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
			nameof(EditorProvider), typeof(IEditorProvider), typeof(PropertyEditorPanel), new PropertyMetadata (default(IEditorProvider), (o, args) => ((PropertyEditorPanel)o).OnEditorProviderChanged()));

		public IEditorProvider EditorProvider
		{
			get { return (IEditorProvider) GetValue (EditorProviderProperty); }
			set { SetValue (EditorProviderProperty, value); }
		}

		private static readonly DependencyPropertyKey SelectedItemsPropertyKey = DependencyProperty.RegisterReadOnly (
			nameof(SelectedItems), typeof(IList), typeof(PropertyEditorPanel), new PropertyMetadata (default(IList)));

		public static readonly DependencyProperty SelectedItemsProperty = SelectedItemsPropertyKey.DependencyProperty;

		public IList SelectedItems
		{
			get { return (IList) GetValue (SelectedItemsProperty); }
			private set { SetValue (SelectedItemsPropertyKey, value); }
		}

		public static readonly DependencyProperty IsArrangeEnabledProperty = DependencyProperty.Register (
			nameof(IsArrangeEnabled), typeof(bool), typeof(PropertyEditorPanel), new PropertyMetadata (true));

		public bool IsArrangeEnabled
		{
			get { return (bool) GetValue (IsArrangeEnabledProperty); }
			set { SetValue (IsArrangeEnabledProperty, value); }
		}

		public static readonly DependencyProperty ArrangeModeProperty = DependencyProperty.Register (
			nameof(ArrangeMode), typeof(PropertyArrangeMode), typeof(PropertyEditorPanel), new PropertyMetadata (PropertyArrangeMode.Name, (o, args) => ((PropertyEditorPanel)o).OnArrangeModeChanged ((PropertyArrangeMode)args.NewValue)));

		public PropertyArrangeMode ArrangeMode
		{
			get { return (PropertyArrangeMode) GetValue (ArrangeModeProperty); }
			set { SetValue (ArrangeModeProperty, value); }
		}

		public static PropertyEditing.Themes.WinThemeManager ThemeManager = new PropertyEditing.Themes.WinThemeManager();

		public override void OnApplyTemplate ()
		{
			base.OnApplyTemplate ();

			this.root = (FrameworkElement) GetTemplateChild ("root");
			this.items = (ItemsControl) GetTemplateChild ("propertyItems");
			this.propertiesPane = (FrameworkElement) GetTemplateChild ("propertiesPane");
			this.eventsPane = (FrameworkElement) GetTemplateChild ("eventsPane");
			this.paneSelector = (ChoiceControl) GetTemplateChild ("paneSelector");
			this.paneSelector.SelectedValue = EditingPane.Properties;
			this.paneSelector.SelectedItemChanged += OnPaneChanged;
			OnEditorProviderChanged();

			if (this.vm.SelectedObjects.Count > 0)
				OnArrangeModeChanged (ArrangeMode);
		}

		private FrameworkElement root;
		private PanelViewModel vm;
		private ItemsControl items;
		private ChoiceControl paneSelector;
		private FrameworkElement propertiesPane, eventsPane;

		private void OnPaneChanged (object sender, EventArgs e)
		{
			object selected = this.paneSelector.SelectedValue;
			EditingPane pane = EditingPane.Properties;
			if (selected != null)
				pane = (EditingPane)selected;

			if (pane == EditingPane.Properties) {
				this.eventsPane.Visibility = Visibility.Collapsed;
				this.propertiesPane.Visibility = Visibility.Visible;
			} else if (pane == EditingPane.Events) {
				this.propertiesPane.Visibility = Visibility.Collapsed;
				this.eventsPane.Visibility = Visibility.Visible;
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

			if (ArrangeMode == PropertyArrangeMode.Name)
				OnArrangeModeChanged (ArrangeMode);
		}

		private void OnEditorProviderChanged ()
		{
			if (this.root == null)
				return;

			if (this.vm != null)
				this.vm.PropertyChanged -= OnVmPropertyChanged;

			this.root.DataContext = this.vm = (EditorProvider != null) ? new PanelViewModel (EditorProvider) : null;
			
			if (this.vm != null)
				this.vm.PropertyChanged += OnVmPropertyChanged;
		}

		private void OnVmPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(PanelViewModel.ArrangeMode))
				SetCurrentValue (ArrangeModeProperty, this.vm.ArrangeMode);
		}

		private void OnArrangeModeChanged (PropertyArrangeMode newMode)
		{
			if (this.items == null)
				return;

			Binding itemsSource;
			if (newMode == PropertyArrangeMode.Name)
				itemsSource = new Binding ("ArrangedProperties[0]");
			else
				itemsSource = new Binding ("ArrangedProperties");

			this.items.SetBinding (ItemsControl.ItemsSourceProperty, itemsSource);
		}
	}

	internal enum EditingPane
	{
		Properties,
		Events
	}
}