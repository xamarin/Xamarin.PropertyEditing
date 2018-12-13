using System.Collections.Generic;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class CharPropertyViewModel
		: PropertyViewModel<char>
	{
		public CharPropertyViewModel (TargetPlatform platform, IPropertyInfo property, IEnumerable<IObjectEditor> editors, PropertyVariation variation = null)
			: base (platform, property, editors, variation)
		{
		}
	}
}
