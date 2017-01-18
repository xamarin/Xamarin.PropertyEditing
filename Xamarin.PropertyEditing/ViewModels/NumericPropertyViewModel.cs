using System.Collections.Generic;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class NumericPropertyViewModel
		: ConstrainedPropertyViewModel<int>
	{
		public NumericPropertyViewModel (IPropertyInfo property, IEnumerable<IObjectEditor> editors)
			: base (property, editors)
		{
		}

		protected override int IncrementValue (int value)
		{
			return value + 1;
		}

		protected override int DecrementValue (int value)
		{
			return value - 1;
		}
	}
}