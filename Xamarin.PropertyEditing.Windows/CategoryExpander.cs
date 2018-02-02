using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
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

		public static readonly DependencyProperty IsFilteredProperty = DependencyProperty.Register (
			"IsFiltered", typeof(bool), typeof(CategoryExpander), new PropertyMetadata (false, (o, args) => ((CategoryExpander)o).OnIsFilteredChanged()));

		public bool IsFiltered
		{
			get { return (bool) GetValue (IsFilteredProperty); }
			set { SetValue (IsFilteredProperty, value); }
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

		protected override void OnKeyDown (KeyEventArgs e)
		{
			if (e.Key == Key.Down) {
				SetExpanded (true);
				UpdateValue();
				e.Handled = true;
			} else if (e.Key == Key.Up) {
				SetExpanded (false);
				UpdateValue();
				e.Handled = true;
			} else if (e.Key == Key.Space) { // Expander should have this built, not sure why it's not working
				SetExpanded (!IsExpanded);
				UpdateValue();
				e.Handled = true;
			}

			base.OnKeyDown (e);
		}

		private bool fromFilter;
		private PanelViewModel panelVm;

		private void UpdateValue ()
		{
			if (this.panelVm == null)
				return;

			SetCurrentValue (IsExpandedProperty, IsFiltered || this.panelVm.GetIsExpanded (Header as string));
		}

		private void OnIsFilteredChanged ()
		{
			this.fromFilter = true;
			UpdateValue ();
			this.fromFilter = false;
		}

		private void UpdateViewModel ()
		{
			FrameworkElement element = this;
			while (element != null && !(element is ItemsControl)) {
				element = VisualTreeHelper.GetParent (element) as FrameworkElement;
			}

			if (element == null) {
				ClearValue (IsFilteredProperty);
				this.panelVm = null;
				return;
			}

			this.panelVm = element.DataContext as PanelViewModel;
			if (this.panelVm == null)
				throw new InvalidOperationException ("Couldn't find valid parent");

			SetBinding (IsFilteredProperty, new Binding (nameof(PanelViewModel.IsFiltering)) {
				Source = this.panelVm
			});
		}

		private void SetExpanded (bool isExpanded)
		{
			if (this.fromFilter)
				return;

			this.panelVm.SetIsExpanded ((string) Header, isExpanded);
		}

		private void OnDataContextChanged (object sender, DependencyPropertyChangedEventArgs e)
		{
			UpdateViewModel();
			UpdateValue();
		}
    }
}
