using System;
using System.Collections.Generic;

using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class PropertyGroupedEditorSelector
		: PropertyEditorSelector
	{
		public override IEditorView GetEditor (IHostResourceProvider hostResources, EditorViewModel vm)
		{
			if (hostResources == null)
				throw new ArgumentNullException (nameof (hostResources));

			Type propertyType = vm.GetType ();
			if (GroupedViewModelTypes.TryGetValue (propertyType, out Type nativeEditorType)) {
				return (IEditorView)Activator.CreateInstance (nativeEditorType, hostResources);
			}

			return base.GetEditor (hostResources, vm);
		}

		private static readonly Dictionary<Type, Type> GroupedViewModelTypes = new Dictionary<Type, Type> {
			{typeof(BrushPropertyViewModel), typeof(BrushTabViewController)}
		};
	}
}
