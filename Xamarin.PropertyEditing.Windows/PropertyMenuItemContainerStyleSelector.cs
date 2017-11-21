using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Xamarin.PropertyEditing.Windows
{
	internal class PropertyMenuItemContainerStyleSelector
		: StyleSelector
	{
		public Style MenuItemStyle
		{
			get;
			set;
		}

		public Style SeparatorStyle
		{
			get;
			set;
		}

		public override Style SelectStyle (object item, DependencyObject container)
		{
			if (container is MenuItem)
				return MenuItemStyle;
			if (container is Separator)
				return SeparatorStyle ?? base.SelectStyle (item, container);

			return base.SelectStyle (item, container);
		}
	}
}
