using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Tests
{
	[TestFixture]
	internal class IntegerPropertyViewModelTests
		: NumericViewModelTests<long, long>
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

		protected override NumericPropertyViewModel<long> GetViewModel (TargetPlatform platform, IPropertyInfo property, IEnumerable<IObjectEditor> editors)
		{
			return new NumericPropertyViewModel<long> (platform, property, editors);
		}
	}

	[TestFixture]
	internal class NullableIntegerPropertyViewModelTests
		: NumericViewModelTests<long?, long>
	{
		// TODO: we can handle value disagreements with null as well

		[Test]
		[Description ("Even if we say its nullable, the IPropertyInfo type and source are the actual controllers")]
		public void ConstrainedToNonNull ()
		{
			var property = GetPropertyMock ();
			// As long as IPropertyInfo Type is non-nullable AND we report supporting the Default source, we should act non-nullable
			property.SetupGet (ip => ip.ValueSources).Returns (ValueSources.Local | ValueSources.Default);
			property.SetupGet (ip => ip.Type).Returns (typeof(long)); // override base long?

			var vm = GetViewModel (property.Object, new MockObjectEditor (property.Object));
			Assert.That (vm.Value, Is.EqualTo (0));

			vm.Value = null;
			Assert.That (vm.Value, Is.EqualTo (0));
		}

		[Test]
		public void NonNullableButUnset ()
		{
			var property = GetPropertyMock ();
			// Even if we say we're non-nullable, if we need to support Unset (!Default), we should act nullable.
			property.SetupGet (ip => ip.ValueSources).Returns (ValueSources.Local);
			property.SetupGet (ip => ip.Type).Returns (typeof(long)); // override base long?

			var vm = GetViewModel (property.Object, new MockObjectEditor (property.Object));
			Assert.That (vm.Value, Is.EqualTo (null));

			vm.Value = 5;
			Assume.That (vm.Value, Is.EqualTo (5));

			vm.Value = null;
			Assert.That (vm.Value, Is.EqualTo (null));
		}

		[Test]
		public void NullableEvenWithDefault ()
		{
			var property = GetPropertyMock ();
			// If the property says its nullable, its nullable.
			property.SetupGet (ip => ip.ValueSources).Returns (ValueSources.Local | ValueSources.Default);
			property.SetupGet (ip => ip.Type).Returns (typeof(long?)); // override base long?

			var vm = GetViewModel (property.Object, new MockObjectEditor (property.Object));
			Assert.That (vm.Value, Is.EqualTo (null));

			vm.Value = 5;
			Assume.That (vm.Value, Is.EqualTo (5));

			vm.Value = null;
			Assert.That (vm.Value, Is.EqualTo (null));
		}

		protected override Tuple<long?, long?> MaxMin => new Tuple<long?, long?> (Int64.MaxValue, Int64.MinValue);

		protected override long? GetRandomTestValue (Random rand)
		{
			return rand.Next ();
		}

		protected override long? GetConstrainedRandomValue (Random rand, out long? max, out long? min)
		{
			int value = rand.Next (2, Int32.MaxValue - 2);
			max = rand.Next (value + 1, Int32.MaxValue);
			min = rand.Next (0, value - 1);
			return value;
		}

		protected override long? GetConstrainedRandomValueAboveBounds (Random rand, out long? max, out long? min)
		{
			int value = rand.Next (2, Int32.MaxValue - 2);
			min = rand.Next (0, value - 1);
			max = rand.Next ((int)min + 1, value - 1);

			return value;
		}

		protected override long? GetConstrainedRandomValueBelowBounds (Random rand, out long? max, out long? min)
		{
			int value = rand.Next (2, Int32.MaxValue - 2);
			max = rand.Next (value + 1, Int32.MaxValue);
			min = rand.Next (value + 1, (int)max - 1);

			return value;
		}

		protected override NumericPropertyViewModel<long?> GetViewModel (TargetPlatform platform, IPropertyInfo property, IEnumerable<IObjectEditor> editors)
		{
			return new NumericPropertyViewModel<long?> (platform, property, editors);
		}
	}
}
