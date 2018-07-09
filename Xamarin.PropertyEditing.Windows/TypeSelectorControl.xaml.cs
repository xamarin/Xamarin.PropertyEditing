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

		public static readonly DependencyProperty ShowTypeLevelProperty = DependencyProperty.Register (
			"ShowTypeLevel", typeof(bool), typeof(TypeSelectorControl), new PropertyMetadata (default(bool)));

		public bool ShowTypeLevel
		{
			get { return (bool) GetValue (ShowTypeLevelProperty); }
			set { SetValue (ShowTypeLevelProperty, value); }
		}

		public static readonly DependencyProperty TypeLevelProperty = DependencyProperty.Register (
			"TypeLevel", typeof(int), typeof(TypeSelectorControl), new PropertyMetadata (default(int)));

		public int TypeLevel
		{
			get { return (int) GetValue (TypeLevelProperty); }
			set { SetValue (TypeLevelProperty, value); }
		}
	}
}
