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

			if (item is KeyValuePair<string, CommonBrushType> choiceItem) {
				switch (choiceItem.Value) {
				case CommonBrushType.NoBrush:
					return NoBrushTemplate;
				case CommonBrushType.Solid:
					return SolidBrushTemplate;
				case CommonBrushType.Resource:
					return ResourceBrushTemplate;
				case CommonBrushType.MaterialDesign:
					return MaterialDesignBrushTemplate;
				}
			}

			return base.SelectTemplate (item, container);
		}

	}
}
