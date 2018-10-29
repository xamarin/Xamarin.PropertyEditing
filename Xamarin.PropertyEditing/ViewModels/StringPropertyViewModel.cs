using System.Collections.Generic;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class StringPropertyViewModel
		: PropertyViewModel<string>
	{
		public StringPropertyViewModel (TargetPlatform platform, IPropertyInfo property, IEnumerable<IObjectEditor> editors, PropertyVariation variation = null)
			: base (platform, property, editors, variation)
		{
		}
	}
}
