using System;
using System.Collections;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Windows
{
	[TemplatePart (Name = "addVariant", Type = typeof(ButtonBase))]
	[TemplatePart (Name = "removeVariant", Type = typeof (ButtonBase))]
	internal class PropertyPresenter
		: ContentControl
	{
		static PropertyPresenter ()
		{
			FocusableProperty.OverrideMetadata (typeof (PropertyPresenter), new FrameworkPropertyMetadata (false));
		}

		public PropertyPresenter()
		{
			Loaded += OnLoaded;
			Unloaded += OnUnloaded;
			DataContextChanged += OnDataContextChanged;
		}

		public static readonly DependencyProperty LabelProperty = DependencyProperty.Register (
			nameof(Label), typeof(object), typeof(PropertyPresenter), new PropertyMetadata (default(object)));

		public object Label
		{
			get { return (object)GetValue (LabelProperty); }
			set { SetValue (LabelProperty, value); }
		}

		public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register (
			nameof(ItemsSource), typeof(IEnumerable), typeof(PropertyPresenter), new PropertyMetadata ());

		public IEnumerable ItemsSource
		{
			get { return (IEnumerable)GetValue (ItemsSourceProperty); }
			set { SetValue (ItemsSourceProperty, value); }
		}

		private static readonly DependencyPropertyKey IsSubPropertyPropertyKey = DependencyProperty.RegisterReadOnly (
			nameof (IsSubProperty), typeof(bool), typeof(PropertyPresenter), new PropertyMetadata());

		public static readonly DependencyProperty IsSubPropertyProperty = IsSubPropertyPropertyKey.DependencyProperty;

		public bool IsSubProperty
		{
			get { return (bool)GetValue (IsSubPropertyProperty); }
			private set { SetValue (IsSubPropertyPropertyKey, value); }
		}

		public static readonly DependencyProperty ShowPropertyButtonProperty = DependencyProperty.Register (
			nameof (ShowPropertyButton), typeof (bool), typeof (PropertyPresenter), new PropertyMetadata (true));

		public bool ShowPropertyButton
		{
			get { return (bool)GetValue (ShowPropertyButtonProperty); }
			set { SetValue (ShowPropertyButtonProperty, value); }
		}

		public static readonly DependencyProperty ShowVariantsProperty = DependencyProperty.Register (
			"ShowVariants", typeof(bool), typeof(PropertyPresenter), new PropertyMetadata (true));

		public bool ShowVariants
		{
			get { return (bool) GetValue (ShowVariantsProperty); }
			set { SetValue (ShowVariantsProperty, value); }
		}

		public static readonly DependencyProperty LineBrushProperty = DependencyProperty.Register (
			"LineBrush", typeof (Brush), typeof (PropertyPresenter), new FrameworkPropertyMetadata (Brushes.White, FrameworkPropertyMetadataOptions.AffectsRender));

		public Brush LineBrush
		{
			get { return (Brush)GetValue (LineBrushProperty); }
			set { SetValue (LineBrushProperty, value); }
		}

		public static readonly DependencyProperty LineThicknessProperty = DependencyProperty.Register (
			"LineThickness", typeof (double), typeof (PropertyPresenter), new FrameworkPropertyMetadata (1d, FrameworkPropertyMetadataOptions.AffectsRender));

		public static readonly DependencyProperty VariantBackgroundBrushProperty = DependencyProperty.Register (
			"VariantBackgroundBrush", typeof(Brush), typeof(PropertyPresenter), new FrameworkPropertyMetadata (default(Brush), FrameworkPropertyMetadataOptions.AffectsRender));

		public Brush VariantBackgroundBrush
		{
			get { return (Brush) GetValue (VariantBackgroundBrushProperty); }
			set { SetValue (VariantBackgroundBrushProperty, value); }
		}

		public double LineThickness
		{
			get { return (double)GetValue (LineThicknessProperty); }
			set { SetValue (LineThicknessProperty, value); }
		}

		public override void OnApplyTemplate ()
		{
			this.addButton = GetTemplateChild ("addVariant") as ButtonBase;
			this.removeButton = GetTemplateChild ("removeVariant") as ButtonBase;
			if (this.removeButton != null) {
				this.removeButton.Click += OnRemoveVariantClicked;
			}

			this.variationRow = (RowDefinition) GetTemplateChild ("variationRow");
			this.variationsList = GetTemplateChild ("variationsList") as ItemsControl;

			this.propertyButton = (PropertyButton) GetTemplateChild ("propertyButton");

			this.propertyContainer = (Border) GetTemplateChild ("propertyContainer");
			// Since the template never changes, the handler only gets added once and there's no need to unsubscribe
			this.propertyContainer.AddHandler (Border.PreviewKeyDownEvent, new KeyEventHandler (PropertyContainer_PreviewKeyDown));
		}

		protected override AutomationPeer OnCreateAutomationPeer ()
		{
			return new PropertyPresenterAutomationPeer (this);
		}

		protected override void OnRender (DrawingContext drawingContext)
		{
			if (this.pvm == null)
				return;

			base.OnRender (drawingContext);
			if (!this.pvm.HasVariantChildren && !this.pvm.IsVariant)
				return;

			drawingContext.DrawRectangle (VariantBackgroundBrush, null, new Rect (0, 0, ActualWidth, ActualHeight));

			var pen = new Pen (LineBrush, LineThickness);
			double centerLeft = Math.Round (Padding.Left / 2);

			if (this.pvm.HasVariantChildren) {
				if (this.addButton == null)
					throw new InvalidOperationException ("addVariant button missing from PropertyPresenter template");

				Point start = this.addButton.TranslatePoint (new Point (this.addButton.ActualWidth / 2, this.addButton.ActualHeight), this);
				drawingContext.DrawLine (pen, new Point (centerLeft, start.Y), new Point (centerLeft, ActualHeight));
			} else if (this.pvm.IsVariant) {
				if (this.removeButton == null)
					throw new InvalidOperationException ("removeButton button missing from PropertyPresenter template");

				Point start = new Point (centerLeft, 0);
				Point end = new Point (centerLeft, (this.pvm.GetIsLastVariant ()) ? this.variationRow.ActualHeight / 2 : ActualHeight);
				drawingContext.DrawLine (pen, start, end);
				start = new Point (centerLeft, this.variationRow.ActualHeight / 2);
				end = this.removeButton.TranslatePoint (new Point (0, this.removeButton.ActualHeight / 2), this);
				drawingContext.DrawLine (pen, start, new Point (end.X, start.Y));
				start = new Point (end.X + this.removeButton.ActualWidth, start.Y);
				end = this.variationsList.TranslatePoint (new Point (0, 0), this);
				drawingContext.DrawLine (pen, start, new Point (end.X, start.Y));
			}
		}

		private void PropertyContainer_PreviewKeyDown (object sender, KeyEventArgs e)
		{
			var isModifierControl = Keyboard.Modifiers == ModifierKeys.Control;

			if (e.Key == Key.Space && isModifierControl) {
				propertyButton.ShowMenu ();
				e.Handled = true;
			}
		}

		private PropertyViewModel pvm;
		private ButtonBase addButton, removeButton;
		private RowDefinition variationRow;
		private ItemsControl variationsList;
		private Border propertyContainer;
		private PropertyButton propertyButton;

		private void OnLoaded (object sender, RoutedEventArgs e)
		{
			IsSubProperty = this.FindParentUnless<PropertyPresenter, PropertyEditorPanel>() != null;
		}

		private void OnUnloaded (object sender, RoutedEventArgs e)
		{
			IsSubProperty = false;
		}

		private void OnDataContextChanged (object sender, DependencyPropertyChangedEventArgs e)
		{
			if (this.pvm != null) {
				this.pvm.CreateVariantRequested -= OnCreateVariantRequested;
			}

			this.pvm = e.NewValue as PropertyViewModel;
			if (this.pvm != null && this.pvm.RequestCreateVariantCommand.CanExecute (null)) {
				this.pvm.CreateVariantRequested += OnCreateVariantRequested;
			}

			(UIElementAutomationPeer.FromElement (this) as PropertyPresenterAutomationPeer)?.Refresh();
		}

		private void OnCreateVariantRequested (object sender, CreateVariantEventArgs e)
		{
			var variation = CreateVariantWindow.RequestVariant (this, this.pvm.Property);
			if (variation != null)
				e.Variation = Task.FromResult (variation);
		}

		private void OnRemoveVariantClicked (object sender, RoutedEventArgs e)
		{
			var parent = (FrameworkElement) VisualTreeHelper.GetParent (this);
			while (!(parent != null && parent.DataContext is PanelGroupViewModel)) {
				parent = (FrameworkElement) VisualTreeHelper.GetParent (parent);
			}

			// Ensure we re-draw background and lines if we removed a variant
			if (parent != null) {
				bool start = false;
				int count = VisualTreeHelper.GetChildrenCount (parent);
				for (int i = 0; i < count; i++) {
					var element = (FrameworkElement) VisualTreeHelper.GetChild (parent, i);
					if (element.DataContext == DataContext)
						continue;

					if (element.DataContext is PropertyViewModel elementVm) {
						if (Equals (elementVm.Property, this.pvm.Property)) {
							start = true;
							element = element.FindChildOrSelf<PropertyPresenter> ();
							element.InvalidateVisual ();
						} else if (start)
							return;
					}
				}
			}
		}

		private class PropertyPresenterAutomationPeer
			: UIElementAutomationPeer
		{
			public PropertyPresenterAutomationPeer (PropertyPresenter owner)
				: base (owner)
			{
				this.presenter = owner;
				Refresh();
			}

			public void Refresh ()
			{
				this.name = AutomationProperties.GetName (this.presenter);
				if (String.IsNullOrEmpty (this.name))
					this.name = (this.presenter.DataContext as PropertyViewModel)?.Name;
			}

			protected override AutomationControlType GetAutomationControlTypeCore ()
			{
				return AutomationControlType.Group;
			}

			protected override string GetNameCore ()
			{
				return this.name;
			}

			private readonly PropertyPresenter presenter;
			private string name;
		}
	}
}