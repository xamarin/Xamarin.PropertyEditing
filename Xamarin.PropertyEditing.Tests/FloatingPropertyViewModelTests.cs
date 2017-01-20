using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Tests
{
	[TestFixture]
	internal class FloatingPropertyViewModelTests
		: ConstrainedPropertyViewModelTests<double>
	{
		protected override double GetRandomTestValue (Random rand)
		{
			return rand.NextDouble ();
		}

		protected override PropertyViewModel<double> GetViewModel (IPropertyInfo property, IEnumerable<IObjectEditor> editors)
		{
			return new FloatingPropertyViewModel (property, editors);
		}

		protected override Tuple<double, double> MaxMin => new Tuple<double, double> (Double.MaxValue, Double.MinValue);

		protected override double GetConstrainedRandomValue (Random rand, out double max, out double min)
		{
			int value = rand.Next (2, Int32.MaxValue - 2);
			max = rand.Next (value + 1, Int32.MaxValue);
			min = rand.Next (0, value - 1);
			return value;
		}

		protected override double GetConstrainedRandomValueAboveBounds (Random rand, out double max, out double min)
		{
			int value = rand.Next (2, Int32.MaxValue - 2);
			min = rand.Next (0, value - 1);
			max = rand.Next ((int)min + 1, value - 1);

			return value;
		}

		protected override double GetConstrainedRandomValueBelowBounds (Random rand, out double max, out double min)
		{
			int value = rand.Next (2, Int32.MaxValue - 2);
			max = rand.Next (value + 1, Int32.MaxValue);
			min = rand.Next (value + 1, (int)max - 1);

			return value;
		}
	}
}
