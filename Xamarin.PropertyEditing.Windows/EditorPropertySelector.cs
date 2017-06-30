using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Windows
{
	internal class EditorTreeSelectorOptions
		: DependencyObject
	{
		public static readonly DependencyProperty ParentTemplateProperty = DependencyProperty.Register (
			"ParentTemplate", typeof(DataTemplate), typeof(EditorTreeSelectorOptions), new PropertyMetadata (default(DataTemplate)));

		public DataTemplate ParentTemplate
		{
			get { return (DataTemplate) GetValue (ParentTemplateProperty); }
			set { SetValue (ParentTemplateProperty, value); }
		}

		public static readonly DependencyProperty EditorTemplateProperty = DependencyProperty.Register (
			"EditorTemplate", typeof(DataTemplate), typeof(EditorTreeSelectorOptions), new PropertyMetadata (default(DataTemplate)));

		public DataTemplate EditorTemplate
		{
			get { return (DataTemplate) GetValue (EditorTemplateProperty); }
			set { SetValue (EditorTemplateProperty, value); }
		}
	}


	internal class EditorTreeSelector
		: DataTemplateSelector
	{
		public EditorTreeSelectorOptions Options
		{
			get;
			set;
		}

		public override DataTemplate SelectTemplate (object item, DependencyObject container)
		{
			var vm = item as PropertyViewModel;
			if (vm != null) {
				if (!vm.CanDelve)
					return Options.EditorTemplate;
				else
					return Options.ParentTemplate;
			}

			return Options.ParentTemplate;
		}
	}

	internal class EditorPropertySelector
		: DataTemplateSelector
	{
		public override DataTemplate SelectTemplate (object item, DependencyObject container)
		{
			if (item != null) {
				Type type = item.GetType ();
				DataTemplate template;
				if (!TryGetTemplate (type, out template)) {
					if (type.IsConstructedGenericType) {
						type = type.GetGenericTypeDefinition ();
						TryGetTemplate (type, out template);
					}
				}

				if (template != null)
					return template;
			}

			return base.SelectTemplate (item, container);
		}

		private readonly Dictionary<Type, DataTemplate> templates = new Dictionary<Type, DataTemplate> ();

		private bool TryGetTemplate (Type type, out DataTemplate template)
		{
			if (this.templates.TryGetValue (type, out template))
				return true;

			Type controlType;
			if (TypeMap.TryGetValue (type, out controlType)) {
				this.templates[type] = template = new DataTemplate (type) {
					VisualTree = new FrameworkElementFactory (controlType)
				};

				return true;
			}

			return false;
		}

		// We can improve on this, but it'll get us started
		private static readonly Dictionary<Type, Type> TypeMap = new Dictionary<Type, Type> {
			{ typeof(StringPropertyViewModel), typeof(StringEditorControl) },
			{ typeof(PropertyViewModel<bool>), typeof(BoolEditorControl) },
			{ typeof(IntegerPropertyViewModel), typeof(NumericEditorControl) },
			{ typeof(FloatingPropertyViewModel), typeof(NumericEditorControl) },
			{ typeof(PropertyViewModel<Point>), typeof(PointEditorControl) },
			{ typeof(PropertyViewModel<Size>), typeof(SizeEditorControl) },
			{ typeof(PropertyViewModel<Thickness>), typeof(ThicknessEditorControl) },
			{ typeof(PredefinedValuesViewModel<>), typeof(EnumEditorControl) }
		};
	}
}
