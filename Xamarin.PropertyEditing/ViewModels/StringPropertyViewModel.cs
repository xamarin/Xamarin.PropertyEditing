using System.Collections.Generic;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class StringPropertyViewModel
		: PropertyViewModel<string>
	{
		public StringPropertyViewModel (IPropertyInfo property, IEnumerable<IObjectEditor> editors)
			: base (property, editors)
		{
		}
	}
}
