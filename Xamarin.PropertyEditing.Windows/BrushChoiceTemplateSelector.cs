using System.Windows;
using System.Windows.Controls;

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

		public override DataTemplate SelectTemplate (object item, DependencyObject container)
		{
			if (item == null)
				return base.SelectTemplate (item, container);

			if (item is ChoiceItem choiceItem) {
				var choice = choiceItem.Value as string;
				if (choice == BrushTabbedEditorControl.None) return NoBrushTemplate;
				if (choice == BrushTabbedEditorControl.Solid) return SolidBrushTemplate;
			}

			return base.SelectTemplate (item, container);
		}
	}
}
