using System;
using System.Collections.Generic;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Tests
{
	class BoolViewModelTests
		: PropertyViewModelTests<bool>
	{
		protected override bool GetRandomTestValue (Random rand)
		{
			return true; // Because we often need to look for change notices, we can't use the default value of the type
		}

		protected override PropertyViewModel<bool> GetViewModel (IPropertyInfo property, IEnumerable<IObjectEditor> editors)
		{
			return new PropertyViewModel<bool> (property, editors);
		}
	}
}
