using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.PropertyEditing.Drawing;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Tests
{
	internal class SizeViewModelTests
		: PropertyViewModelTests<CommonSize, SizePropertyViewModel>
	{
		[Test]
		public void Width ()
		{
			var property = GetPropertyMock ();
			var editor = GetBasicEditor (property.Object);
			var vm = GetViewModel (property.Object, new[] { editor });
			Assume.That (vm.Value, Is.EqualTo (new CommonSize (0, 0)));

			bool xChanged = false, valueChanged = false;
			vm.PropertyChanged += (sender, args) => {
				if (args.PropertyName == nameof(SizePropertyViewModel.Width))
					xChanged = true;
				if (args.PropertyName == nameof(SizePropertyViewModel.Value))
					valueChanged = true;
			};

			vm.Width = 5;
			Assert.That (vm.Value.Width, Is.EqualTo (5));
			Assert.That (xChanged, Is.True);
			Assert.That (valueChanged, Is.True);
		}

		[Test]
		public void Height ()
		{
			var property = GetPropertyMock ();
			var editor = GetBasicEditor (property.Object);
			var vm = GetViewModel (property.Object, new[] { editor });
			Assume.That (vm.Value, Is.EqualTo (new CommonSize (0, 0)));

			bool yChanged = false, valueChanged = false;
			vm.PropertyChanged += (sender, args) => {
				if (args.PropertyName == nameof(SizePropertyViewModel.Height))
					yChanged = true;
				if (args.PropertyName == nameof(SizePropertyViewModel.Value))
					valueChanged = true;
			};

			vm.Height = 5;
			Assert.That (vm.Value.Height, Is.EqualTo (5));
			Assert.That (yChanged, Is.True);
			Assert.That (valueChanged, Is.True);
		}

		[Test]
		public void ValueChangesWidthHeight ()
		{
			var property = GetPropertyMock ();
			var editor = GetBasicEditor (property.Object);
			var vm = GetViewModel (property.Object, new[] { editor });
			Assume.That (vm.Width, Is.EqualTo (0));
			Assume.That (vm.Height, Is.EqualTo (0));

			bool xChanged = false, yChanged = false, valueChanged = false;
			vm.PropertyChanged += (sender, args) => {
				if (args.PropertyName == nameof(SizePropertyViewModel.Width))
					xChanged = true;
				if (args.PropertyName == nameof(SizePropertyViewModel.Height))
					yChanged = true;
				if (args.PropertyName == nameof(SizePropertyViewModel.Value))
					valueChanged = true;
			};

			vm.Value = new CommonSize (5, 10);

			Assert.That (vm.Width, Is.EqualTo (5));
			Assert.That (vm.Height, Is.EqualTo (10));
			Assert.That (yChanged, Is.True);
			Assert.That (xChanged, Is.True);
			Assert.That (valueChanged, Is.True);
		}

		protected override CommonSize GetRandomTestValue (Random rand)
		{
			return new CommonSize (rand.Next (), rand.Next ());
		}

		protected override SizePropertyViewModel GetViewModel (IPropertyInfo property, IEnumerable<IObjectEditor> editors)
		{
			return new SizePropertyViewModel (property, editors);
		}
	}
}
