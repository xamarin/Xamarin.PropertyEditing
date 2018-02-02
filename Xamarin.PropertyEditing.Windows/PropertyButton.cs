using System;
using System.Linq;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
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

		public static readonly DependencyProperty SystemResourcesSourceProperty = DependencyProperty.Register (
			"SystemResourcesSource", typeof (IEnumerable), typeof (PropertyButton), new PropertyMetadata (default (IEnumerable)));

		public IEnumerable SystemResourcesSource
		{
			get { return (IEnumerable)GetValue (SystemResourcesSourceProperty); }
			set { SetValue (SystemResourcesSourceProperty, value); }
		}

		public static readonly DependencyProperty SystemResourceNamePathProperty = DependencyProperty.Register (
			"SystemResourceNamePath", typeof (string), typeof (PropertyButton), new PropertyMetadata ("Name"));

		public string SystemResourceNamePath
		{
			get { return (string)GetValue (SystemResourceNamePathProperty); }
			set { SetValue (SystemResourceNamePathProperty, value); }
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
			var pvm = e.OldValue as PropertyViewModel;
			if (pvm != null)
				pvm.ResourceRequested -= OnResourceRequested;

			pvm = e.NewValue as PropertyViewModel;
			if (pvm != null)
				pvm.ResourceRequested += OnResourceRequested;
		}

		private void OnValueSourceChanged (ValueSource source)
		{
			if (this.indicator == null)
				return;

			string brush = null;

			switch (source) {
				case ValueSource.Local:
					brush = "PropertyLocalValueBrush";
					break;
				case ValueSource.Binding:
					brush = "PropertyBoundValueBrush";
					break;
				case ValueSource.Inherited:
				case ValueSource.DefaultStyle:
				case ValueSource.Style:
				case ValueSource.Resource:
					brush = "PropertyResourceBrush";
					break;

				case ValueSource.Default:
				case ValueSource.Unset:
					this.indicator.ClearValue (Shape.FillProperty);
					return;
			}

			this.indicator.SetResourceReference (Shape.FillProperty, brush);
		}

		private void OnResourceRequested (object sender, ResourceRequestedEventArgs e)
		{
			var vm = ((PropertyViewModel)DataContext);
			e.Resource = ResourceSelectorWindow.RequestResource (Window.GetWindow (this), vm.ResourceProvider, vm.Editors.Select (ed => ed.Target), vm.Property);
		}

		private void OnCustomExpression (object sender, RoutedEventArgs e)
		{
			var popup = new EntryPopup {
				Placement = PlacementMode.Bottom,
				PlacementTarget = this.indicator,
				DataContext = DataContext
			};

			popup.SetResourceReference (Popup.StyleProperty, "CustomExpressionPopup");
			popup.IsOpen = true;
		}
	}
}