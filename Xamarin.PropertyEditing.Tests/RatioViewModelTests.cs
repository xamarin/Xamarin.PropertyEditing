using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.PropertyEditing.Drawing;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Tests
{
	internal class RatioViewModelTests
		: PropertyViewModelTests<CommonRatio, RatioViewModel>
	{
		[Test]
		public void Numerator ()
		{
			var property = GetPropertyMock ();
			var editor = GetBasicEditor (property.Object);
			var vm = GetViewModel (property.Object, new[] { editor });
			Assume.That (vm.Value, Is.EqualTo (new CommonRatio ()));

			bool xChanged = false, valueChanged = false;
			vm.PropertyChanged += (sender, args) => {
				if (args.PropertyName == nameof(RatioViewModel.Numerator))
					xChanged = true;
				if (args.PropertyName == nameof(RatioViewModel.Value))
					valueChanged = true;
			};

			vm.Numerator = 5;
			Assert.That (vm.Value.Numerator, Is.EqualTo (5));
			Assert.That (xChanged, Is.True);
			Assert.That (valueChanged, Is.True);
		}

		[Test]
		public void Denominator ()
		{
			var property = GetPropertyMock ();
			var editor = GetBasicEditor (property.Object);
			var vm = GetViewModel (property.Object, new[] { editor });
			Assume.That (vm.Value, Is.EqualTo (new CommonRatio ()));

			bool xChanged = false, valueChanged = false;
			vm.PropertyChanged += (sender, args) => {
				if (args.PropertyName == nameof (RatioViewModel.Denominator))
					xChanged = true;
				if (args.PropertyName == nameof (RatioViewModel.Value))
					valueChanged = true;
			};

			vm.Denominator = 5;
			Assert.That (vm.Value.Denominator, Is.EqualTo (5));
			Assert.That (xChanged, Is.True);
			Assert.That (valueChanged, Is.True);
		}

		[Test]
		public void RatioSeparator ()
		{
			var property = GetPropertyMock ();
			var editor = GetBasicEditor (property.Object);
			var vm = GetViewModel (property.Object, new[] { editor });
			Assume.That (vm.Value, Is.EqualTo (new CommonRatio ()));

			bool xChanged = false, valueChanged = false;
			vm.PropertyChanged += (sender, args) => {
				if (args.PropertyName == nameof (RatioViewModel.RatioSeparator))
					xChanged = true;
				if (args.PropertyName == nameof (RatioViewModel.Value))
					valueChanged = true;
			};

			vm.RatioSeparator = '/';
			Assert.That (vm.Value.RatioSeparator, Is.EqualTo ('/'));
			Assert.That (xChanged, Is.True);
			Assert.That (valueChanged, Is.True);
		}

		[Test]
		public void ValueChangesNumeratorDenominatorRatioSeparator ()
		{
			var property = GetPropertyMock ();
			var editor = GetBasicEditor (property.Object);
			var vm = GetViewModel (property.Object, new[] { editor });
			Assume.That (vm.Numerator, Is.EqualTo (1));
			Assume.That (vm.Denominator, Is.EqualTo (1));

			bool nChanged = false, dChanged = false, sChanged = false, valueChanged = false;
			vm.PropertyChanged += (sender, args) => {
				if (args.PropertyName == nameof(RatioViewModel.Numerator))
					nChanged = true;
				if (args.PropertyName == nameof(RatioViewModel.Denominator))
					dChanged = true;
				if (args.PropertyName == nameof (RatioViewModel.RatioSeparator))
					sChanged = true;
				if (args.PropertyName == nameof(SizePropertyViewModel.Value))
					valueChanged = true;
			};

			vm.Value = new CommonRatio (5, 10, ':');

			Assert.That (vm.Numerator, Is.EqualTo (5));
			Assert.That (vm.Denominator, Is.EqualTo (10));
			Assert.That (nChanged, Is.True);
			Assert.That (dChanged, Is.True);
			Assert.That (sChanged, Is.True);
			Assert.That (valueChanged, Is.True);
		}

		[Test]
		public void RatioWithoutDenominator ()
		{
			var property = GetPropertyMock ();
			var editor = GetBasicEditor (property.Object);
			var vm = GetViewModel (property.Object, new[] { editor });

			vm.ValueString = "21";

			Assert.That (vm.Numerator, Is.EqualTo (21));
			Assert.That (vm.Denominator, Is.EqualTo (1));
			Assert.That (vm.RatioSeparator, Is.EqualTo (':'));
		}

		[Test]
		public void RatioWithNumeratorDenominatorAndColonSeparator ()
		{
			var property = GetPropertyMock ();
			var editor = GetBasicEditor (property.Object);
			var vm = GetViewModel (property.Object, new[] { editor });

			vm.ValueString = "21:2";

			Assert.That (vm.Numerator, Is.EqualTo (21));
			Assert.That (vm.Denominator, Is.EqualTo (2));
			Assert.That (vm.RatioSeparator, Is.EqualTo (':'));
		}

		[Test]
		public void RatioWithNumeratorDenominatorAndSlashSeparator ()
		{
			var property = GetPropertyMock ();
			var editor = GetBasicEditor (property.Object);
			var vm = GetViewModel (property.Object, new[] { editor });

			vm.ValueString = "21/2";

			Assert.That (vm.Numerator, Is.EqualTo (21));
			Assert.That (vm.Denominator, Is.EqualTo (2));
			Assert.That (vm.RatioSeparator, Is.EqualTo ('/'));
		}

		[Test]
		public void RatioWithWrongSeparator ()
		{
			var property = GetPropertyMock ();
			var editor = GetBasicEditor (property.Object);
			var vm = GetViewModel (property.Object, new[] { editor });
			bool eChanged = false;
			vm.ErrorsChanged += (sender, args) => {
				eChanged = true;
			};

			vm.ValueString = "21@2";

			// Was Error raised?
			Assert.That (eChanged, Is.True);
		}

		[Test]
		public void RatioWithSpaces ()
		{
			var property = GetPropertyMock ();
			var editor = GetBasicEditor (property.Object);
			var vm = GetViewModel (property.Object, new[] { editor });

			vm.ValueString = " 21: 2 ";

			Assert.That (vm.ValueString, Is.EqualTo ("21:2"));
		}

		protected override CommonRatio GetRandomTestValue (Random rand)
		{
			var randomDenominator = rand.Next ();
			// Can't be less than 1
			if (randomDenominator < 1) 
				randomDenominator = 1;
			return new CommonRatio (rand.Next (), randomDenominator, '/');
		}

		protected override RatioViewModel GetViewModel (TargetPlatform platform, IPropertyInfo property, IEnumerable<IObjectEditor> editors)
		{
			return new RatioViewModel (platform, property, editors);
		}
	}
}
