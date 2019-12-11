using System;
using System.Linq;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Xamarin.PropertyEditing.ViewModels;
using System.Windows.Automation;
using System.Reflection;

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

		public void ShowMenu ()
		{
			if (this.menu == null) {
				this.menu = MenuTemplate?.LoadContent () as ContextMenu;
				if (this.menu == null)
					return;

				this.menu.PlacementTarget = this.indicator;
				this.menu.DataContext = DataContext;

				MenuItem customExpression = this.menu.Items.OfType<MenuItem>().FirstOrDefault (mi => mi.Name == "CustomExpressionItem");
				if (customExpression != null)
					customExpression.Click += OnCustomExpression;

				if (!initializedSetPositionInSetMethod) {
					setPositionInSetMethod = typeof (AutomationProperties).GetMethod ("SetPositionInSet");
					initializedSetPositionInSetMethod = true;
				}

				// .NET Framework 4.8 added the PositionInSet automation property. It also changed the semantics so that if this
				// property is not set it's computed automatically. And it's read by the narrator. The net result is that when
				// proppy is run in a .NET Framework 4.8 app (like Visual Studio), the narrator reads off the position of menu items
				// (e.g. "blah blah menu item 4 of 11"). AND hidden menu items are counted here, which makes those positions wrong,
				// causing this bug: https://devdiv.visualstudio.com/DevDiv/_workitems/edit/1000455.
				// To work around all of that, we set the PositionInSet property explicitly, when running on .NET Framework 4.8,
				// to be 0. That causes the narrator not to read the position at all, which is how most VS menus behave.
				if (setPositionInSetMethod != null) {
					foreach (object item in this.menu.Items) {
						if (item is MenuItem menuItem)
							setPositionInSetMethod.Invoke (null, new object[] { menuItem, 0 });
					}
				}
			}

			this.menu.IsOpen = true;
		}

		protected override AutomationPeer OnCreateAutomationPeer ()
		{
			return new PropertyButtonAutomationPeer (this);
		}

		private Rectangle indicator;
		private ContextMenu menu;
		private PropertyViewModel vm;
		private static bool initializedSetPositionInSetMethod;
		private static MethodInfo setPositionInSetMethod;

		private void OnBorderMouseDown (object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton != MouseButton.Left)
				return;

			ShowMenu();
			e.Handled = true;
		}

		private void OnDataContextChanged (object sender, DependencyPropertyChangedEventArgs e)
		{
			if (this.vm != null) {
				this.vm.PropertyChanged -= OnPropertyChanged;
				this.vm.ResourceRequested -= OnResourceRequested;
				this.vm.CreateBindingRequested -= OnCreateBindingRequested;
				this.vm.CreateResourceRequested -= OnCreateResourceRequested;
			}

			this.vm = e.NewValue as PropertyViewModel;
			if (this.vm != null) {
				this.vm.PropertyChanged += OnPropertyChanged;
				this.vm.ResourceRequested += OnResourceRequested;
				this.vm.CreateBindingRequested += OnCreateBindingRequested;
				this.vm.CreateResourceRequested += OnCreateResourceRequested;
			}
		}

		private void OnPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName) {
			case nameof(PropertyViewModel.Resource):
				OnValueSourceChanged (this.vm.ValueSource);
				break;
			}
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
					ToolTip = Properties.Resources.Binding;
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

		private void OnCreateBindingRequested (object sender, CreateBindingRequestedEventArgs e)
		{
			var panel = this.FindPropertiesHost ();
			var pvm = (PropertyViewModel) DataContext;

			e.BindingObject = CreateBindingWindow.CreateBinding (panel, pvm.TargetPlatform, pvm.Editors.Single(), pvm.Property);
		}

		private void OnResourceRequested (object sender, ResourceRequestedEventArgs e)
		{
			var panel = this.FindPropertiesHost();
			var pvm = (PropertyViewModel) DataContext;
			e.Resource = ResourceSelectorWindow.RequestResource (panel, pvm.TargetPlatform.ResourceProvider, pvm.Editors.Select (ed => ed.Target), pvm.Property, e.Resource);
		}

		private void OnCreateResourceRequested (object sender, CreateResourceRequestedEventArgs e)
		{
			var panel = this.FindPropertiesHost();
			var pvm = (PropertyViewModel) DataContext;

			var result = CreateResourceWindow.CreateResource (panel, pvm.TargetPlatform.ResourceProvider, pvm.Editors.Select (oe => oe.Target), pvm.Property);
			e.Source = result.Item1;
			e.Name = result.Item2;
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

		private class PropertyButtonAutomationPeer
			: FrameworkElementAutomationPeer, IInvokeProvider
		{
			public PropertyButtonAutomationPeer (PropertyButton button)
				: base (button)
			{
				Button = button;
			}

			protected PropertyButton Button
			{
				get;
			}

			public void Invoke ()
			{
				Button.ShowMenu ();
			}

			public override object GetPattern (PatternInterface patternInterface)
			{
				if (patternInterface == PatternInterface.Invoke)
					return this;

				return base.GetPattern (patternInterface);
			}

			protected override bool IsControlElementCore ()
			{
				return Button.IsVisible;
			}

			protected override string GetClassNameCore ()
			{
				return nameof(PropertyButton);
			}

			protected override AutomationControlType GetAutomationControlTypeCore ()
			{
				return AutomationControlType.Button;
			}
		}
	}
}