using System;
using System.Collections.Generic;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class TimeSpanPropertyViewModel
		: PropertyViewModel<TimeSpan>
	{
		public TimeSpanPropertyViewModel (TargetPlatform platform, IPropertyInfo property, IEnumerable<IObjectEditor> editors, PropertyVariation variation = null)
			: base (platform, property, editors, variation)
		{
		}
	}
}
