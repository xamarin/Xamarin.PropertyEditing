using System.Collections.Generic;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class StringPropertyViewModel
		: PropertyViewModel<string>
	{
		public StringPropertyViewModel (TargetPlatform platform, IPropertyInfo property, IEnumerable<IObjectEditor> editors, PropertyVariationSet variant = null)
			: base (platform, property, editors, variant)
		{
		}
	}
}
