using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Windows
{
	internal class EditorPropertySelector
		: DataTemplateSelector
	{
		public override DataTemplate SelectTemplate (object item, DependencyObject container)
		{
			if (item != null) {
				Type type = item.GetType ();

				DataTemplate template = null;
				if (!this.templates.TryGetValue (type, out template)) {
					Type controlType;
					if (TypeMap.TryGetValue (type, out controlType)) {
						this.templates[type] = template = new DataTemplate (type) {
							VisualTree = new FrameworkElementFactory (controlType)
						};
						template.VisualTree.SetBinding (PropertyEditorControl.LabelProperty, new Binding ("Property.Name") { Mode = BindingMode.OneTime });
					}
				}

				if (template != null)
					return template;
			}

			return base.SelectTemplate (item, container);
		}

		private readonly Dictionary<Type, DataTemplate> templates = new Dictionary<Type, DataTemplate> ();

		// We can improve on this, but it'll get us started
		private static readonly Dictionary<Type, Type> TypeMap = new Dictionary<Type, Type> {
			{ typeof(StringPropertyViewModel), typeof(StringEditorControl) }
		};
	}
}
