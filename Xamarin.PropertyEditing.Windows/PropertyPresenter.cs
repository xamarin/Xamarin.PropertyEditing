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

		protected override Size ArrangeOverride (Size arrangeBounds)
		{
			return base.ArrangeOverride (arrangeBounds);
		}

		protected override Size MeasureOverride (Size constraint)
		{
			return base.MeasureOverride (constraint);
		}
	}
}