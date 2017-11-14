using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Xamarin.PropertyEditing.Windows
{
	[TemplatePart (Name = "List", Type = typeof(ItemsControl))]
	[TemplatePart (Name = "EditorPresenter", Type = typeof(Selector))]
	internal class GroupEditorControl
		: Selector
	{
		public GroupEditorControl ()
		{
			FocusableProperty.OverrideMetadata (typeof(GroupEditorControl), new FrameworkPropertyMetadata (false));

			ItemContainerGenerator.StatusChanged += OnItemContainerGeneratorOnStatusChanged;
		}

		public static readonly DependencyProperty ContentTemplateProperty = DependencyProperty.Register (
			"ContentTemplate", typeof(DataTemplate), typeof(GroupEditorControl), new PropertyMetadata (default(DataTemplate)));

		public DataTemplate ContentTemplate
		{
			get { return (DataTemplate) GetValue (ContentTemplateProperty); }
			set { SetValue (ContentTemplateProperty, value); }
		}

		public static readonly DependencyProperty ContentTemplateSelectorProperty = DependencyProperty.Register (
			"ContentTemplateSelector", typeof(DataTemplateSelector), typeof(GroupEditorControl), new PropertyMetadata (default(DataTemplateSelector)));

		public DataTemplateSelector ContentTemplateSelector
		{
			get { return (DataTemplateSelector) GetValue (ContentTemplateSelectorProperty); }
			set { SetValue (ContentTemplateSelectorProperty, value); }
		}

		public static readonly DependencyProperty SelectorStyleProperty = DependencyProperty.Register (
			"SelectorStyle", typeof(Style), typeof(GroupEditorControl), new PropertyMetadata (default(Style)));

		public Style SelectorStyle
		{
			get { return (Style) GetValue (SelectorStyleProperty); }
			set { SetValue (SelectorStyleProperty, value); }
		}

		protected override void OnSelectionChanged (SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count == 0)
				return;

			var presenter = (ContentPresenter) ItemContainerGenerator.ContainerFromItem (e.AddedItems[0]);
			if (presenter == null || VisualTreeHelper.GetChildrenCount (presenter) == 0)
				return;

			var toggle = (ToggleButton) VisualTreeHelper.GetChild (presenter, 0);
			toggle.IsChecked = true;
		}

		protected override void OnItemsChanged (NotifyCollectionChangedEventArgs e)
		{
			base.OnItemsChanged (e);

			if (SelectedItem == null && Items.Count > 0)
				SelectedIndex = 0;
		}

		protected override void OnKeyDown (KeyEventArgs e)
		{
			base.OnKeyDown (e);

			if (e.Key == Key.Down && SelectedIndex < Items.Count - 1)
				SetCurrentValue (SelectedIndexProperty, SelectedIndex + 1);
			else if (e.Key == Key.Up && SelectedIndex >= 1)
				SetCurrentValue (SelectedIndexProperty, SelectedIndex - 1);
		}

		private void OnItemContainerGeneratorOnStatusChanged (object sender, EventArgs args)
		{
			if (ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
				return;

			if (SelectedItem == null)
				SetCurrentValue (SelectedItemProperty, Items[0]);

			for (int i = 0; i < Items.Count; i++) {
				var container = ItemContainerGenerator.ContainerFromIndex (i) as ContentPresenter;
				if (container == null)
					throw new InvalidOperationException ("Unexpected visual tree");

				container.ApplyTemplate ();

				var child = VisualTreeHelper.GetChild (container, 0);
				var toggle = child as ToggleButton;
				if (toggle == null)
					throw new InvalidOperationException ("Children must be of ToggleButton");

				if (Equals (SelectedItem, container.DataContext))
					toggle.IsChecked = true;

				toggle.Checked -= OnChoiceSelected;
				toggle.Checked += OnChoiceSelected;
			}
		}

		private void OnChoiceSelected (object sender, RoutedEventArgs e)
		{
			object selected = ((FrameworkElement) sender).DataContext;

			SetCurrentValue (SelectedItemProperty, selected);
		}
	}
}
