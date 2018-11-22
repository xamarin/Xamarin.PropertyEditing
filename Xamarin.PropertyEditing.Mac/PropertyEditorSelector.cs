using System;
using System.Collections.Generic;
using System.Drawing;

using Xamarin.PropertyEditing.Drawing;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class PropertyEditorSelector
	{
		public virtual IEditorView GetEditor (EditorViewModel vm)
		{
			Type[] genericArgs = null;
			Type controlType;
			Type propertyType = vm.GetType ();
			if (!ViewModelTypes.TryGetValue (propertyType, out controlType)) {
				if (propertyType.IsConstructedGenericType) {
					genericArgs = propertyType.GetGenericArguments ();
					propertyType = propertyType.GetGenericTypeDefinition ();
					ViewModelTypes.TryGetValue (propertyType, out controlType);
				}
			}

			if (controlType == null)
				return null;

			if (controlType.IsGenericTypeDefinition) {
				if (genericArgs == null)
					genericArgs = propertyType.GetGenericArguments ();

				controlType = controlType.MakeGenericType (genericArgs);
			}

			return (IEditorView)Activator.CreateInstance (controlType);
		}

		private static readonly Dictionary<Type, Type> ViewModelTypes = new Dictionary<Type, Type> {
			{typeof (StringPropertyViewModel), typeof (StringEditorControl)},
			{typeof (NumericPropertyViewModel<>), typeof (NumericEditorControl<>)},
			{typeof (PropertyViewModel<bool?>), typeof (BooleanEditorControl)},
			{typeof (PredefinedValuesViewModel<>), typeof(PredefinedValuesEditor<>)},
			{typeof (CombinablePropertyViewModel<>), typeof(CombinablePropertyEditor<>)},
			{typeof (PropertyViewModel<CoreGraphics.CGPoint>), typeof (CGPointEditorControl)},
			{typeof (PropertyViewModel<CoreGraphics.CGRect>), typeof (CGRectEditorControl)},
			{typeof (PropertyViewModel<CoreGraphics.CGSize>), typeof (CGSizeEditorControl)},
			{typeof (PointPropertyViewModel), typeof (CommonPointEditorControl) },
			{typeof (RectanglePropertyViewModel), typeof (CommonRectangleEditorControl) },
			{typeof (SizePropertyViewModel), typeof (CommonSizeEditorControl) },
			{typeof (PropertyViewModel<Point>), typeof (SystemPointEditorControl)},
			{typeof (PropertyViewModel<Size>), typeof (SystemSizeEditorControl)},
			{typeof (PropertyViewModel<Rectangle>), typeof (SystemRectangleEditorControl)},
			{typeof (BrushPropertyViewModel), typeof (BrushEditorControl)},
			{typeof (RatioViewModel), typeof (RatioEditorControl<CommonRatio>)},
			{typeof (ThicknessPropertyViewModel), typeof (CommonThicknessEditorControl) },
			{typeof (PropertyGroupViewModel), typeof (GroupEditorControl)},
			{typeof (DateTimePropertyViewModel), typeof (DateTimeEditorControl) },
		};
	}
}
