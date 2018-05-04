using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Xamarin.PropertyEditing.Drawing;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Windows
{
	/// <remarks>
	/// While technically it might be reasonable for the template to be relative to the container, its not a scenario
	/// we currently need so it's faster to cache the template in a global capacity. Container relative can be achieved
	/// by putting a new instance of the selector 
	/// </remarks>
	internal class EditorTreeSelector
		: DataTemplateSelector
	{
		public object EditorTemplateKey { get; set; }
		public object ParentTemplateKey { get; set; }

		public override DataTemplate SelectTemplate (object item, DependencyObject container)
		{
			if (item is EditorViewModel) {
				if (this.editorTemplate == null)
					this.editorTemplate = (DataTemplate)((FrameworkElement)container).TryFindResource (EditorTemplateKey);

				return this.editorTemplate;
			}

			if (this.parentTemplate == null)
				this.parentTemplate = (DataTemplate)((FrameworkElement)container).TryFindResource (ParentTemplateKey);

			return this.parentTemplate;
		}

		private DataTemplate editorTemplate, parentTemplate;
	}

	internal class GroupedEditorPropertySelector
		: EditorPropertySelector
	{
		protected override bool TryGetTemplateType (Type type, out Type templateType)
		{
			if (TypeMap.TryGetValue (type, out templateType))
				return true;

			return base.TryGetTemplateType (type, out templateType);
		}

		private static readonly EditorPropertySelector selector;

		private static readonly Dictionary<Type, Type> TypeMap = new Dictionary<Type, Type> {
			{ typeof(BrushPropertyViewModel), typeof(BrushTabbedEditorControl) }
		};
	}

	internal class EditorPropertySelector
		: DataTemplateSelector
	{
		public override DataTemplate SelectTemplate (object item, DependencyObject container)
		{
			if (item != null) {
				Type type = item.GetType ();
				if (!TryGetTemplate (type, out DataTemplate template)) {
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

		protected virtual bool TryGetTemplateType (Type type, out Type templateType)
		{
			return TypeMap.TryGetValue (type, out templateType);
		}

		private readonly Dictionary<Type, DataTemplate> templates = new Dictionary<Type, DataTemplate> ();

		private bool TryGetTemplate (Type type, out DataTemplate template)
		{
			if (this.templates.TryGetValue (type, out template))
				return true;

			if (TryGetTemplateType (type, out Type controlType)) {
				this.templates[type] = template = new DataTemplate (type) {
					VisualTree = new FrameworkElementFactory (controlType)
				};

				return true;
			}

			return false;
		}

		private static readonly Dictionary<Type, Type> TypeMap = new Dictionary<Type, Type> {
			{ typeof(StringPropertyViewModel), typeof(StringEditorControl) },
			{ typeof(PropertyViewModel<bool?>), typeof(BoolEditorControl) },
			{ typeof(NumericPropertyViewModel<>), typeof(NumericEditorControl) },
			{ typeof(PointPropertyViewModel), typeof(PointEditorControl) },
			{ typeof(SizePropertyViewModel), typeof(SizeEditorControl) },
			{ typeof(ThicknessPropertyViewModel), typeof(ThicknessEditorControl) },
			{ typeof(PredefinedValuesViewModel<>), typeof(EnumEditorControl) },
			{ typeof(CombinablePropertyViewModel<>), typeof(CombinablePredefinedValuesEditor) },
			{ typeof(BrushPropertyViewModel), typeof(BrushEditorControl) },
			{ typeof(PropertyGroupViewModel), typeof(GroupEditorControl) },
			{ typeof(ObjectPropertyViewModel), typeof(ObjectEditorControl) },
			{ typeof(CollectionPropertyViewModel), typeof(CollectionEditor) },

		};
	}
}
