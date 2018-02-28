using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Xamarin.PropertyEditing.Windows
{
	internal class ChoiceItem
		: NotifyingObject
	{
		public string Name
		{
			get { return this.name; }
			set
			{
				if (this.name == value)
					return;

				this.name = value;
				OnPropertyChanged();
			}
		}

		public string Tooltip
		{
			get { return this.tooltip; }
			set
			{
				if (this.tooltip == value)
					return;

				this.tooltip = value;
				OnPropertyChanged();
			}
		}

		public object Value
		{
			get { return this.value; }
			set
			{
				if (this.value == value)
					return;

				this.value = value;
				OnPropertyChanged();
			}
		}

		private object value;
		private string tooltip;
		private string name;
	}

	[TemplatePart (Name = "list", Type = typeof(ListBox))]
	internal class ChoiceControl
		: Selector
	{
		public ChoiceControl ()
		{
			ItemContainerGenerator.StatusChanged += OnItemContainerGeneratorOnStatusChanged;
		}

		public event EventHandler SelectedItemChanged;

		protected override void OnSelectionChanged (SelectionChangedEventArgs e)
		{
			if (e.RemovedItems.Count > 0) {
				var removedToggle = GetToggle (e.RemovedItems[0]);
				if (removedToggle != null)
					removedToggle.IsChecked = false;
			}

			if (e.AddedItems.Count > 0) {
				var addedToggle = GetToggle (e.AddedItems[0]);
				if (addedToggle != null)
					addedToggle.IsChecked = true;
			}
		}

		internal void FocusSelectedItem ()
		{
			var container = ItemContainerGenerator.ContainerFromIndex (SelectedIndex) as ContentPresenter;
			if (container == null)
				throw new InvalidOperationException ("Unexpected visual tree");

			var toggle = VisualTreeHelper.GetChild (container, 0) as ToggleButton;
			if (toggle == null)
				throw new InvalidOperationException ("Children must be of ToggleButton");
			toggle.Focus ();
		}

		private ToggleButton GetToggle (object item)
		{
			var presenter = (ContentPresenter) ItemContainerGenerator.ContainerFromItem (item);
			if (presenter == null)
				return null;

			return VisualTreeHelper.GetChild (presenter, 0) as ToggleButton;
		}

		private void OnItemContainerGeneratorOnStatusChanged (object sender, EventArgs args)
		{
			if (ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated || (ItemTemplate == null && ItemTemplateSelector == null))
				return;

			// Note: this does not handle a changing items ItemsSource. It's not something that's currently required and unlikely to be.
			for (int i = 0; i < Items.Count; i++) {
				var container = ItemContainerGenerator.ContainerFromIndex (i) as ContentPresenter;
				if (container == null)
					throw new InvalidOperationException ("Unexpected visual tree");

				container.ApplyTemplate ();

				var child = VisualTreeHelper.GetChild (container, 0);
				var toggle = child as ToggleButton;
				if (toggle == null)
					throw new InvalidOperationException ("Children must be of ToggleButton. This exception may be caused by an error in the template, an uninitialized template, or a disposed template.");

				if (Equals (SelectedItem, container.DataContext))
					toggle.IsChecked = true;

				toggle.Checked += OnChoiceSelected;
			}
		}

		private void OnChoiceSelected (object sender, RoutedEventArgs e)
		{
			object selected = ((FrameworkElement) sender).DataContext;

			SetCurrentValue (SelectedItemProperty, selected);
			SelectedItemChanged?.Invoke (sender, EventArgs.Empty);
		}
	}
}
