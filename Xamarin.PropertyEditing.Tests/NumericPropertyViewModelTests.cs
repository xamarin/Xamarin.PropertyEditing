using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Tests
{
	[TestFixture]
	internal class NumericPropertyViewModelTests
		: ConstrainedPropertyViewModelTests<int>
	{
		protected override int GetRandomTestValue (Random rand)
		{
			return rand.Next ();
		}

		protected override int GetConstrainedRandomValue (Random rand, out int max, out int min)
		{
			int value = rand.Next (2, Int32.MaxValue - 2);
			max = rand.Next (value + 1, Int32.MaxValue);
			min = rand.Next (0, value - 1);
			return value;
		}

		protected override int GetConstrainedRandomValueAboveBounds (Random rand, out int max, out int min)
		{
			int value = rand.Next (2, Int32.MaxValue - 2);
			min = rand.Next (0, value - 1);
			max = rand.Next (min + 1, value - 1);

			return value;
		}

		protected override int GetConstrainedRandomValueBelowBounds (Random rand, out int max, out int min)
		{
			int value = rand.Next (2, Int32.MaxValue - 2);
			max = rand.Next (value + 1, Int32.MaxValue);
			min = rand.Next (value + 1, max - 1);

			return value;
		}

		protected override PropertyViewModel<int> GetViewModel (IPropertyInfo property, IEnumerable<IObjectEditor> editors)
		{
			return new NumericPropertyViewModel (property, editors);
		}
	}
}
