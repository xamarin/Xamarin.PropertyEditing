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
			var presenter = (ContentPresenter) ItemContainerGenerator.ContainerFromItem (e.AddedItems[0]);
			if (presenter == null)
				return;

			var toggle = (ToggleButton) VisualTreeHelper.GetChild (presenter, 0);
			toggle.IsChecked = true;
		}

		private void OnItemContainerGeneratorOnStatusChanged (object sender, EventArgs args)
		{
			if (ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
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
					throw new InvalidOperationException ("Children must be of ToggleButton");

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
