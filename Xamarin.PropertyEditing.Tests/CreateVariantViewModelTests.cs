using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Tests
{
	[TestFixture]
	class CreateVariantViewModelTests
	{
		[Test]
		public void AnySelectedByDefault ()
		{
			var property = GetTestProperty (out PropertyVariation[] variations);
			var vm = new VariationViewModel ("Width", new[] { variations[0], variations[1] });
			Assert.That (vm.SelectedVariation, Is.Not.Null);
			Assert.That (vm.IsAnySelected, Is.True);
		}

		[Test]
		public void IsAnySelectedUpdates ()
		{
			var property = GetTestProperty (out PropertyVariation[] variations);
			var vm = new VariationViewModel ("Width", new[] { variations[0], variations[1] });
			Assume.That (vm.SelectedVariation, Is.Not.Null);
			Assume.That (vm.IsAnySelected, Is.True);

			bool changed = false;
			vm.PropertyChanged += (sender, args) => {
				if (args.PropertyName == nameof (VariationViewModel.IsAnySelected))
					changed = true;
			};

			vm.SelectedVariation = variations[0];
			Assert.That (vm.IsAnySelected, Is.False, "IsAnySelected did not switch to false");
			Assert.That (changed, Is.True, "PropertyChanged did not fire for IsAnySelected");

			changed = false;
			vm.SelectedVariation = vm.Variations[0];
			Assert.That (vm.IsAnySelected, Is.True, "IsAnySelected did not switch to back to true");
			Assert.That (changed, Is.True, "PropertyChanged did not fire for IsAnySelected");
		}

		[Test]
		public void VariantCategories ()
		{
			var property = GetTestProperty (out PropertyVariation[] variations);
			var categories = variations.Select (v => v.Category).Distinct ().ToArray();
			var vm = new CreateVariantViewModel (property.Object);
			Assert.That (vm.VariationCategories.Count, Is.EqualTo (categories.Length));
			CollectionAssert.AreEqual (vm.VariationCategories.Select (v => v.Name), categories);
		}

		[Test]
		public void WhenAllAnyCommandDisabledEnabled ()
		{
			var property = GetTestProperty (out PropertyVariation[] variations);
			var vm = new CreateVariantViewModel (property.Object);
			Assume.That (vm.VariationCategories.All (vvm => vvm.IsAnySelected), Is.True);
			Assert.That (vm.CreateVariantCommand.CanExecute (null), Is.False);

			bool changed = false;
			vm.CreateVariantCommand.CanExecuteChanged += (sender, args) => changed = true;

			vm.VariationCategories[0].SelectedVariation = vm.VariationCategories[0].Variations[1];
			Assert.That (changed, Is.True, "CanExecuteChanged did not fire");
			Assert.That (vm.CreateVariantCommand.CanExecute (null), Is.True);
		}

		[Test]
		public void CreateVariant ()
		{
			var property = GetTestProperty (out PropertyVariation[] variations);
			var vm = new CreateVariantViewModel (property.Object);
			Assume.That (vm.Variant, Is.Null);

			bool changed = false;
			vm.PropertyChanged += (sender, args) => {
				if (args.PropertyName == nameof (CreateVariantViewModel.Variant))
					changed = true;
			};

			vm.VariationCategories[0].SelectedVariation = vm.VariationCategories[0].Variations[1];
			vm.VariationCategories[1].SelectedVariation = vm.VariationCategories[1].Variations[2];
			vm.CreateVariantCommand.Execute (null);

			Assert.That (changed, Is.True, "Variant did not fire PropertyChanged");
			Assert.That (vm.Variant, Is.Not.Null);
			Assert.That (vm.Variant.Count, Is.EqualTo (2));
			Assert.That (vm.Variant, Contains.Item (vm.VariationCategories[0].Variations[1]));
			Assert.That (vm.Variant, Contains.Item (vm.VariationCategories[1].Variations[2]));
		}

		private Mock<IPropertyInfo> GetTestProperty (out PropertyVariation[] variations)
		{
			variations = new[] {
				new PropertyVariation ("Width", "Compact"),
				new PropertyVariation ("Width", "Regular"),
				new PropertyVariation ("Gamut", "P3"),
				new PropertyVariation ("Gamut", "sRGB"),
				new PropertyVariation ("Other", "Other"),
			};

			var property = new Mock<IPropertyInfo> ();
			property.SetupGet (p => p.Name).Returns ("Variant");
			property.SetupGet (p => p.Type).Returns (typeof (string));
			property.SetupGet (p => p.RealType).Returns (typeof (string).ToTypeInfo ());
			property.SetupGet (p => p.CanWrite).Returns (true);
			property.SetupGet (p => p.ValueSources).Returns (ValueSources.Default | ValueSources.Local);
			property.SetupGet (p => p.Variations).Returns (variations);
			return property;
		}
	}
}