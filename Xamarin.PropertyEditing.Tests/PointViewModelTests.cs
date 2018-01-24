using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.PropertyEditing.Drawing;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Tests
{
	[TestFixture]
	internal class PointViewModelTests
		: PropertyViewModelTests<CommonPoint, PointPropertyViewModel>
	{
		[Test]
		public void X ()
		{
			var property = GetPropertyMock ();
			var editor = GetBasicEditor (property.Object);
			var vm = GetViewModel (property.Object, new[] { editor });
			Assume.That (vm.Value, Is.EqualTo (new CommonPoint (0, 0)));

			bool xChanged = false, valueChanged = false;
			vm.PropertyChanged += (sender, args) => {
				if (args.PropertyName == nameof(PointPropertyViewModel.X))
					xChanged = true;
				if (args.PropertyName == nameof(PointPropertyViewModel.Value))
					valueChanged = true;
			};

			vm.X = 5;
			Assert.That (vm.Value.X, Is.EqualTo (5));
			Assert.That (xChanged, Is.True);
			Assert.That (valueChanged, Is.True);
		}

		[Test]
		public void Y ()
		{
			var property = GetPropertyMock ();
			var editor = GetBasicEditor (property.Object);
			var vm = GetViewModel (property.Object, new[] { editor });
			Assume.That (vm.Value, Is.EqualTo (new CommonPoint (0, 0)));

			bool yChanged = false, valueChanged = false;
			vm.PropertyChanged += (sender, args) => {
				if (args.PropertyName == nameof(PointPropertyViewModel.Y))
					yChanged = true;
				if (args.PropertyName == nameof(PointPropertyViewModel.Value))
					valueChanged = true;
			};

			vm.Y = 5;
			Assert.That (vm.Value.Y, Is.EqualTo (5));
			Assert.That (yChanged, Is.True);
			Assert.That (valueChanged, Is.True);
		}

		[Test]
		public void ValueChangesXY ()
		{
			var property = GetPropertyMock ();
			var editor = GetBasicEditor (property.Object);
			var vm = GetViewModel (property.Object, new[] { editor });
			Assume.That (vm.X, Is.EqualTo (0));
			Assume.That (vm.Y, Is.EqualTo (0));

			bool xChanged = false, yChanged = false, valueChanged = false;
			vm.PropertyChanged += (sender, args) => {
				if (args.PropertyName == nameof(PointPropertyViewModel.X))
					xChanged = true;
				if (args.PropertyName == nameof(PointPropertyViewModel.Y))
					yChanged = true;
				if (args.PropertyName == nameof(PointPropertyViewModel.Value))
					valueChanged = true;
			};

			vm.Value = new CommonPoint (5, 10);

			Assert.That (vm.X, Is.EqualTo (5));
			Assert.That (vm.Y, Is.EqualTo (10));
			Assert.That (yChanged, Is.True);
			Assert.That (xChanged, Is.True);
			Assert.That (valueChanged, Is.True);
		}

		protected override CommonPoint GetRandomTestValue (Random rand)
		{
			return new CommonPoint (rand.Next (), rand.Next ());
		}

		protected override PointPropertyViewModel GetViewModel (TargetPlatform platform, IPropertyInfo property, IEnumerable<IObjectEditor> editors)
		{
			return new PointPropertyViewModel (platform, property, editors);
		}
	}
}
