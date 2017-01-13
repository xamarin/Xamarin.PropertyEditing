using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Tests
{
	[TestFixture]
	internal class StringViewModelTests
		: PropertyViewModelTests<string>
	{
		protected override PropertyViewModel<string> GetViewModel (IPropertyInfo property, IEnumerable<IObjectEditor> editors)
		{
			return new StringPropertyViewModel (property, editors);
		}

		protected override string GetRandomTestValue (Random rand)
		{
			return rand.Next (Int32.MaxValue).ToString ();
		}
	}
}
