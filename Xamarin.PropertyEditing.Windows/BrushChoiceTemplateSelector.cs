using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Xamarin.PropertyEditing.Drawing;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Windows
{
	public class BrushChoiceTemplateSelector
		: DataTemplateSelector
	{
		public DataTemplate NoBrushTemplate {
			get;
			set;
		}

		public DataTemplate SolidBrushTemplate {
			get;
			set;
		}

		public DataTemplate ResourceBrushTemplate
		{
			get;
			set;
		}

		public DataTemplate MaterialDesignBrushTemplate
		{
			get;
			set;
		}

		public override DataTemplate SelectTemplate (object item, DependencyObject container)
		{
			if (item == null)
				return base.SelectTemplate (item, container);

			if (item is KeyValuePair<string, Type> choiceItem) {
				if (choiceItem.Value == null)
					return NoBrushTemplate;
				if (choiceItem.Value == typeof(CommonSolidBrush))
					return SolidBrushTemplate;
				if (choiceItem.Value == typeof(Resource))
					return ResourceBrushTemplate;
				if (choiceItem.Value == typeof(MaterialColorScale))
					return MaterialDesignBrushTemplate;
			}

			return base.SelectTemplate (item, container);
		}

	}
}
