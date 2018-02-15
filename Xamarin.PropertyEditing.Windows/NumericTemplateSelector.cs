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

		public DataTemplate ByteTemplate
		{
			get;
			set;
		}

		public override DataTemplate SelectTemplate (object item, DependencyObject container)
		{
			if (item == null)
				return base.SelectTemplate (item, container);

			if (item is NumericPropertyViewModel<double?> || item is NumericPropertyViewModel<float?>)
				return FloatingTemplate;
			else if (item is NumericPropertyViewModel<long?> || item is NumericPropertyViewModel<int?>)
				return IntegerTemplate;
			else if (item is NumericPropertyViewModel<byte?>)
				return ByteTemplate;

			return base.SelectTemplate (item, container);
		}
	}
}
