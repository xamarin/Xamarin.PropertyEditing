using System;
using System.Collections.Generic;

using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class PropertyGroupedEditorSelector
		: PropertyEditorSelector
	{
		public override IEditorView GetEditor (EditorViewModel vm)
		{
			Type propertyType = vm.GetType ();
			if (GroupedViewModelTypes.TryGetValue (propertyType, out Type nativeEditorType)) {
				return (IEditorView)Activator.CreateInstance (nativeEditorType);
			}

			return base.GetEditor (vm);
		}

		private static readonly Dictionary<Type, Type> GroupedViewModelTypes = new Dictionary<Type, Type> {
			{typeof(BrushPropertyViewModel), typeof(BrushTabViewController)}
		};
	}
}
