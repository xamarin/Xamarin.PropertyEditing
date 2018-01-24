using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Tests
{
	[TestFixture]
	internal class IntegerPropertyViewModelTests
		: ConstrainedPropertyViewModelTests<long, IntegerPropertyViewModel>
	{
		protected override Tuple<long, long> MaxMin => new Tuple<long, long> (Int64.MaxValue, Int64.MinValue);

		protected override long GetRandomTestValue (Random rand)
		{
			return rand.Next ();
		}

		protected override long GetConstrainedRandomValue (Random rand, out long max, out long min)
		{
			int value = rand.Next (2, Int32.MaxValue - 2);
			max = rand.Next (value + 1, Int32.MaxValue);
			min = rand.Next (0, value - 1);
			return value;
		}

		protected override long GetConstrainedRandomValueAboveBounds (Random rand, out long max, out long min)
		{
			int value = rand.Next (2, Int32.MaxValue - 2);
			min = rand.Next (0, value - 1);
			max = rand.Next ((int)min + 1, value - 1);

			return value;
		}

		protected override long GetConstrainedRandomValueBelowBounds (Random rand, out long max, out long min)
		{
			int value = rand.Next (2, Int32.MaxValue - 2);
			max = rand.Next (value + 1, Int32.MaxValue);
			min = rand.Next (value + 1, (int)max - 1);

			return value;
		}

		protected override IntegerPropertyViewModel GetViewModel (TargetPlatform platform, IPropertyInfo property, IEnumerable<IObjectEditor> editors)
		{
			return new IntegerPropertyViewModel (platform, property, editors);
		}
	}
}
