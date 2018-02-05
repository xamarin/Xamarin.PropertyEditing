using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace Xamarin.PropertyEditing.Windows
{
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

		protected override Size ArrangeOverride (Size arrangeBounds)
		{
			return base.ArrangeOverride (arrangeBounds);
		}

		protected override Size MeasureOverride (Size constraint)
		{
			return base.MeasureOverride (constraint);
		}

		private void OnLoaded (object sender, RoutedEventArgs e)
		{
			IsSubProperty = this.FindParentUnless<PropertyPresenter, PropertyEditorPanel>() != null;
		}

		private void OnUnloaded (object sender, RoutedEventArgs e)
		{
			IsSubProperty = false;
		}
	}
}