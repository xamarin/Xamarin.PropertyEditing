using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Windows
{
	internal class CategoryExpander
		: Expander
	{
		public CategoryExpander ()
		{
			DataContextChanged += OnDataContextChanged;
		}

		public override void OnApplyTemplate ()
		{
			base.OnApplyTemplate ();
			UpdateValue();
		}

		protected override void OnExpanded ()
		{
			base.OnExpanded ();
			SetExpanded (true);
		}

		protected override void OnCollapsed ()
		{
			base.OnCollapsed ();
			SetExpanded (false);
		}

		private FrameworkElement parent;

		private void UpdateValue ()
		{
			var vm = GetViewModel ();
			if (vm == null)
				return;

			SetCurrentValue (IsExpandedProperty, vm.GetIsExpanded ((string) Header));
		}

		private PanelViewModel GetViewModel ()
		{
			FrameworkElement element = (this.parent as ItemsControl) ?? (FrameworkElement)this;
			while (!(element is ItemsControl)) {
				element = VisualTreeHelper.GetParent (element) as FrameworkElement;
			}
			this.parent = element;

			if (this.parent == null)
				throw new InvalidOperationException ("Couldn't find valid parent");

			return this.parent.DataContext as PanelViewModel;
		}

		private void SetExpanded (bool isExpanded)
		{
			var vm = GetViewModel ();
			if (vm == null)
				throw new InvalidOperationException ("Couldn't find valid parent");

			vm.SetIsExpanded ((string) Header, isExpanded);
		}

		private void OnDataContextChanged (object sender, DependencyPropertyChangedEventArgs e)
		{
			UpdateValue ();
		}
    }
}
