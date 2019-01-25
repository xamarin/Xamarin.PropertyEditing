using System;
using System.Collections.Generic;

using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.Mac
{
	internal abstract class ValueViewSelector
	{
		public abstract IValueView CreateView (IHostResourceProvider hostResources, Type valueType);
	}

	internal class PropertyInlinePreviewSelector
		: ValueViewSelector
	{
		public override IValueView CreateView (IHostResourceProvider hostResources, Type valueType)
		{
			if (!ValueTypes.TryGetValue (valueType, out Type viewType))
				return null;

			return (IValueView)Activator.CreateInstance (viewType, hostResources);
		}

		private static readonly Dictionary<Type, Type> ValueTypes = new Dictionary<Type, Type> {
			{typeof (CommonBrush), typeof (CommonBrushView)},
			{typeof (CommonColor), typeof (CommonBrushView)},
		};
	}
}
