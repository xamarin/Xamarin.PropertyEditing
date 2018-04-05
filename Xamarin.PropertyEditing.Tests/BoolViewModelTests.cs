using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Tests
{
	[TestFixture]
	internal class BoolViewModelTests
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

	[TestFixture]
	internal class NullableBoolViewModelTests
		: PropertyViewModelTests<bool?, bool, PropertyViewModel<bool?>>
	{
		protected override bool? GetRandomTestValue (Random rand)
		{
			return (rand.Next (0, 2) == 1);
		}

		protected override PropertyViewModel<bool?> GetViewModel (TargetPlatform platform, IPropertyInfo property, IEnumerable<IObjectEditor> editors)
		{
			return new PropertyViewModel<bool?> (platform, property, editors);
		}

		[Test]
		public void NonNullableStillNullsOnDisagreeValues ()
		{
			var prop = new Mock<IPropertyInfo> ();
			prop.SetupGet (pi => pi.Type).Returns (typeof(bool));
			prop.SetupGet (pi => pi.ValueSources).Returns (ValueSources.Local | ValueSources.Default);

			var basicEditor = GetBasicEditor (true, prop.Object);
			var basicEditor2 = GetBasicEditor (false, prop.Object);
			
			var vm = GetViewModel (prop.Object, new [] { basicEditor, basicEditor2 });

			Assert.That (vm.Value, Is.EqualTo (null));
			Assert.That (vm.MultipleValues, Is.True);
		}
	}
}
