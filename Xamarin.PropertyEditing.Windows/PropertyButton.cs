using System;
using System.Linq;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Windows
{
	[TemplatePart (Name = "Border", Type = typeof(UIElement))]
	[TemplatePart (Name = "Indicator", Type = typeof(Rectangle))]
	internal class PropertyButton
		: Control
	{
		static PropertyButton ()
		{
			FocusableProperty.OverrideMetadata (typeof (PropertyButton), new FrameworkPropertyMetadata (false));
			DefaultStyleKeyProperty.OverrideMetadata (typeof (PropertyButton), new FrameworkPropertyMetadata (typeof (PropertyButton)));
		}

		public PropertyButton()
		{
			DataContextChanged += OnDataContextChanged;
		}

		public static readonly DependencyProperty CanSetCustomExpressionProperty = DependencyProperty.Register (
			"CanSetCustomExpression", typeof (bool), typeof (PropertyButton), new PropertyMetadata (default (bool)));

		public bool CanSetCustomExpression
		{
			get { return (bool)GetValue (CanSetCustomExpressionProperty); }
			set { SetValue (CanSetCustomExpressionProperty, value); }
		}

		public static readonly DependencyProperty ValueSourceProperty = DependencyProperty.Register (
			"ValueSource", typeof (ValueSource), typeof (PropertyButton), new PropertyMetadata (ValueSource.Default, (o, args) => ((PropertyButton)o).OnValueSourceChanged ((ValueSource)args.NewValue)));

		public ValueSource ValueSource
		{
			get { return (ValueSource)GetValue (ValueSourceProperty); }
			set { SetValue (ValueSourceProperty, value); }
		}

		public static readonly DependencyProperty MenuTemplateProperty = DependencyProperty.Register (
			"MenuTemplate", typeof (DataTemplate), typeof (PropertyButton), new PropertyMetadata (default (DataTemplate)));

		public DataTemplate MenuTemplate
		{
			get { return (DataTemplate)GetValue (MenuTemplateProperty); }
			set { SetValue (MenuTemplateProperty, value); }
		}

		public override void OnApplyTemplate ()
		{
			base.OnApplyTemplate ();

			this.indicator = (Rectangle)GetTemplateChild ("Indicator");
			if (this.indicator == null)
				throw new InvalidOperationException ("PropertyButton template Missing part Indicator");

			var border = GetTemplateChild ("Border") as UIElement;
			if (border == null)
				throw new InvalidOperationException ("PropertyButton template Missing part Border");

			border.MouseDown += OnBorderMouseDown;

			OnValueSourceChanged (ValueSource);
		}

		private Rectangle indicator;
		private ContextMenu menu;
		private PropertyViewModel vm;

		private void OnBorderMouseDown (object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton != MouseButton.Left)
				return;

			if (this.menu == null) {
				this.menu = MenuTemplate?.LoadContent () as ContextMenu;
				if (this.menu == null)
					return;

				this.menu.PlacementTarget = this.indicator;
				this.menu.DataContext = DataContext;

				MenuItem customExpression = this.menu.Items.OfType<MenuItem>().FirstOrDefault (mi => mi.Name == "CustomExpressionItem");
				if (customExpression != null)
					customExpression.Click += OnCustomExpression;
			}

			this.menu.IsOpen = true;
			e.Handled = true;
		}

		private void OnDataContextChanged (object sender, DependencyPropertyChangedEventArgs e)
		{
			if (e.OldValue is PropertyViewModel pvm)
				pvm.ResourceRequested -= OnResourceRequested;

			this.vm = e.NewValue as PropertyViewModel;
			if (this.vm != null)
				this.vm.ResourceRequested += OnResourceRequested;
		}

		private void OnValueSourceChanged (ValueSource source)
		{
			if (this.indicator == null)
				return;

			switch (source) {
				case ValueSource.Local:
					ToolTip = Properties.Resources.Local;
					break;
				case ValueSource.Binding:
					break;
				case ValueSource.Inherited:
					ToolTip = Properties.Resources.Inherited;
					break;
				case ValueSource.DefaultStyle:
				case ValueSource.Style:
					ToolTip = null;
					break;
				case ValueSource.Resource:
					// VS actually says "static" for system resources, but I think that's worth breaking with
					ToolTip = (this.vm?.Resource?.Name != null) ? String.Format (Properties.Resources.ResourceWithName, this.vm.Resource.Name) : Properties.Resources.Resource;
					break;
				case ValueSource.Default:
					ToolTip = Properties.Resources.Default;
					return;
				case ValueSource.Unset:
					ToolTip = Properties.Resources.Unset;
					return;
			}
		}

		private void OnResourceRequested (object sender, ResourceRequestedEventArgs e)
		{
			var panel = this.FindParent<PropertyEditorPanel> ();
			var vm = ((PropertyViewModel)DataContext);
			e.Resource = ResourceSelectorWindow.RequestResource (panel, vm.ResourceProvider, vm.Editors.Select (ed => ed.Target), vm.Property, e.Resource);
		}

		private void OnCustomExpression (object sender, RoutedEventArgs e)
		{
			var popup = new EntryPopup {
				Placement = PlacementMode.Bottom,
				PlacementTarget = this.indicator,
				DataContext = DataContext,
				StaysOpen = false
			};

			popup.SetResourceReference (Popup.StyleProperty, "CustomExpressionPopup");
			popup.IsOpen = true;
		}
	}
}