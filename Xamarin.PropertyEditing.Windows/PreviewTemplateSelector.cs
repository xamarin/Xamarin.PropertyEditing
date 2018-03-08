using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Xamarin.PropertyEditing.Windows
{
	internal class PreviewTemplateSelector
		: DataTemplateSelector
	{
		public List<DataTemplate> Templates
		{
			get;
		} = new List<DataTemplate> ();

		public DataTemplate FallbackTemplate
		{
			get;
			set;
		}

		public override DataTemplate SelectTemplate (object item, DependencyObject container)
		{
			if (item != null) {
				Type itemType = item.GetType ();
				DataTemplate template = Templates.FirstOrDefault (t => ((Type)t.DataType).IsAssignableFrom (itemType));
				if (template != null)
					return template;
			}

			return FallbackTemplate ?? base.SelectTemplate (item, container);
		}
	}
}
