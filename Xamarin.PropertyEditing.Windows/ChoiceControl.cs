using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Xamarin.PropertyEditing.Windows
{
	[TemplatePart (Name = "list", Type = typeof(ListBox))]
	internal class ChoiceControl
		: Control
	{
		public event EventHandler SelectedItemChanged;

		public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register ("ItemsSource", typeof (IEnumerable), typeof (ChoiceControl));

		public IEnumerable ItemsSource
		{
			get { return (IEnumerable)GetValue (ItemsSourceProperty); }
			set { SetValue (ItemsSourceProperty, value); }
		}

		public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register ("ItemTemplate", typeof (DataTemplate), typeof (ChoiceControl));

		public DataTemplate ItemTemplate
		{
			get { return (DataTemplate)GetValue (ItemTemplateProperty); }
			set { SetValue (ItemTemplateProperty, value); }
		}

		public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register ("SelectedItem", typeof (object), typeof (ChoiceControl));

		public object SelectedItem
		{
			get { return GetValue (SelectedItemProperty); }
			set { SetValue (SelectedItemProperty, value); }
		}

		public override void OnApplyTemplate ()
		{
			base.OnApplyTemplate ();

			this.list = (ListBox)GetTemplateChild ("list");
			this.list.SelectionChanged += OnListSelectionChanged;
		}

		private ListBox list;

		private void OnListSelectionChanged (object sender, SelectionChangedEventArgs e)
		{
			object selectedItem = e.AddedItems.Count > 0 ? e.AddedItems[0] : null;
			SetCurrentValue (SelectedItemProperty, selectedItem);
			SelectedItemChanged?.Invoke (sender, EventArgs.Empty);
		}
	}
}
