using System.Collections.Generic;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class StringPropertyViewModel
		: PropertyViewModel<string>
	{
		public StringPropertyViewModel (TargetPlatform platform, IPropertyInfo property, IEnumerable<IObjectEditor> editors)
			: base (platform, property, editors)
		{
		}
	}
}
