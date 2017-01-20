using System.Windows;
using System.Windows.Controls;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Windows
{
	internal class NumericTemplateSelector
		: DataTemplateSelector
	{
		public DataTemplate IntegerTemplate
		{
			get;
			set;
		}

		public DataTemplate FloatingTemplate
		{
			get;
			set;
		}

		public override DataTemplate SelectTemplate (object item, DependencyObject container)
		{
			if (item == null)
				return base.SelectTemplate (item, container);

			if (item is ConstrainedPropertyViewModel<double> || item is ConstrainedPropertyViewModel<float>)
				return FloatingTemplate;
			else if (item is ConstrainedPropertyViewModel<long> || item is ConstrainedPropertyViewModel<int>)
				return IntegerTemplate;

			return base.SelectTemplate (item, container);
		}
	}
}
