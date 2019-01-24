using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Windows
{
	internal class FilterExpander
		: Expander
	{

		public static readonly DependencyProperty IsFilteredProperty = DependencyProperty.Register (
			"IsFiltered", typeof (bool), typeof (FilterExpander), new PropertyMetadata (false, (o, args) => ((FilterExpander)o).OnIsFilteredChanged ()));

		public bool IsFiltered
		{
			get { return (bool)GetValue (IsFilteredProperty); }
			set { SetValue (IsFilteredProperty, value); }
		}

		public override void OnApplyTemplate ()
		{
			base.OnApplyTemplate ();
			UpdateViewModel();
		}

		protected PanelViewModel ViewModel
		{
			get;
			private set;
		}

		protected virtual void UpdateValue ()
		{
			if (ViewModel == null)
				return;

			SetCurrentValue (IsExpandedProperty, IsFiltered);
		}

		protected virtual void OnIsFilteredChanged ()
		{
			UpdateValue ();
		}

		private void UpdateViewModel ()
		{
			FrameworkElement element = this;
			while (element != null && !(element is ItemsControl)) {
				element = VisualTreeHelper.GetParent (element) as FrameworkElement;
			}

			if (element == null) {
				ClearValue (IsFilteredProperty);
				ViewModel = null;
				return;
			}

			ViewModel = element.DataContext as PanelViewModel;
			if (ViewModel == null)
				throw new InvalidOperationException ("Couldn't find valid parent");

			SetBinding (IsFilteredProperty, new Binding (nameof (PanelViewModel.IsFiltering)) {
				Source = ViewModel
			});

			UpdateValue();
		}
	}
}
