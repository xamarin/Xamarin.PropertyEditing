using System;
using System.Collections.Generic;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Tests
{
	class BoolViewModelTests
		: PropertyViewModelTests<bool, PropertyViewModel<bool>>
	{
		protected override bool GetRandomTestValue (Random rand)
		{
			return (rand.Next (0, 2) == 1);
		}

		protected override PropertyViewModel<bool> GetViewModel (TargetPlatform platform, IPropertyInfo property, IEnumerable<IObjectEditor> editors)
		{
			return new PropertyViewModel<bool> (platform, property, editors);
		}
	}
}
