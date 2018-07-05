using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Xamarin.PropertyEditing.Windows
{
	public partial class TypeSelectorControl
		: UserControl
	{
		public TypeSelectorControl ()
		{
			InitializeComponent ();
		}

		public event RoutedPropertyChangedEventHandler<object> SelectedItemChanged
		{
			add { this.tree.SelectedItemChanged += value; }
			remove { this.tree.SelectedItemChanged -= value; }
		}

		public event EventHandler ItemActivated
		{
			add { this.tree.ItemActivated += value; }
			remove { this.tree.ItemActivated -= value; }
		}

		public override void OnApplyTemplate ()
		{
			base.OnApplyTemplate ();

			this.tree.SelectedItemChanged += OnSelectedItemChanged;
		}

		public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register (
			"SelectedItem", typeof(object), typeof(TypeSelectorControl), new PropertyMetadata (default(object)));

		public object SelectedItem
		{
			get { return (object) GetValue (SelectedItemProperty); }
			set { SetValue (SelectedItemProperty, value); }
		}

		private void OnSelectedItemChanged (object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			SetCurrentValue (SelectedItemProperty, e.NewValue);
		}
	}
}
