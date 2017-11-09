using System.Collections.Specialized;
using Moq;
using NUnit.Framework;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Tests
{
	[TestFixture]
	internal class PropertyGroupViewModelTests
	{
		[Test]
		public void PropertyGroup ()
		{
			IObjectEditor editor = null;

			var prop = new Mock<IPropertyInfo> ();
			prop.SetupGet (p => p.Type).Returns (typeof(int));
		
			var prop2 = new Mock<IPropertyInfo> ();
			prop2.SetupGet (p => p.Type).Returns (typeof(int));

			editor = new MockObjectEditor (prop.Object, prop2.Object);
			var pvm = new PropertyViewModel<int> (prop.Object, new[] { editor });
			var pvm2 = new PropertyViewModel<int> (prop2.Object, new[] { editor });

			var vm = new PropertyGroupViewModel ("category", new[] { pvm, pvm2 }, new [] { editor});
			Assert.That (vm.Properties, Contains.Item (pvm));
			Assert.That (vm.Properties, Contains.Item (pvm2));
		}

		[Test]
		public void UnavailablePropertyNotInList ()
		{
			IObjectEditor editor;

			var constraint = new Mock<IAvailabilityConstraint>();
			var prop = new Mock<IPropertyInfo> ();
			prop.SetupGet (p => p.Type).Returns (typeof(int));
			prop.SetupGet (p => p.AvailabilityConstraints).Returns (new[] { constraint.Object });
			
			var constraint2 = new Mock<IAvailabilityConstraint> ();
			var prop2 = new Mock<IPropertyInfo> ();
			prop2.SetupGet (p => p.Type).Returns (typeof(int));
			prop2.SetupGet (p => p.AvailabilityConstraints).Returns (new[] { constraint2.Object });

			editor = new MockObjectEditor (prop.Object, prop2.Object);
			constraint.Setup (c => c.GetIsAvailableAsync (editor)).ReturnsAsync (true);
			constraint2.Setup (c => c.GetIsAvailableAsync (editor)).ReturnsAsync (false);

			var pvm = new PropertyViewModel<int> (prop.Object, new[] { editor });
			var pvm2 = new PropertyViewModel<int> (prop2.Object, new[] { editor });

			var vm = new PropertyGroupViewModel ("category", new[] { pvm, pvm2 }, new [] { editor});
			Assert.That (vm.Properties, Contains.Item (pvm));
			Assert.That (vm.Properties, Does.Not.Contain (pvm2));
		}

		[Test]
		public void AvailabilityUpdates ()
		{
			IObjectEditor editor = null;

			var constraint = new Mock<IAvailabilityConstraint>();
			var prop = new Mock<IPropertyInfo> ();
			prop.SetupGet (p => p.Type).Returns (typeof(int));
			prop.SetupGet (p => p.AvailabilityConstraints).Returns (new[] { constraint.Object });

			bool isAvailable = false;

			var constraint2 = new Mock<IAvailabilityConstraint> ();
			constraint2.SetupGet (a => a.ConstrainingProperties).Returns (new[] { prop.Object });
			var prop2 = new Mock<IPropertyInfo> ();
			prop2.SetupGet (p => p.Type).Returns (typeof(int));
			prop2.SetupGet (p => p.AvailabilityConstraints).Returns (new[] { constraint2.Object });
			
			editor = new MockObjectEditor (prop.Object, prop2.Object);
			constraint.Setup (c => c.GetIsAvailableAsync (editor)).ReturnsAsync (true);
			constraint2.Setup (c => c.GetIsAvailableAsync (editor)).ReturnsAsync (() => isAvailable);

			var pvm = new PropertyViewModel<int> (prop.Object, new[] { editor });
			var pvm2 = new PropertyViewModel<int> (prop2.Object, new[] { editor });

			var vm = new PropertyGroupViewModel ("category", new[] { pvm, pvm2 }, new [] { editor});
			Assume.That (vm.Properties, Contains.Item (pvm));
			Assume.That (vm.Properties, Does.Not.Contain (pvm2));

			INotifyCollectionChanged notify = vm.Properties as INotifyCollectionChanged;
			Assume.That (notify, Is.Not.Null);

			bool changed = false;
			notify.CollectionChanged += (sender, args) => {
				if (args.Action == NotifyCollectionChangedAction.Add && args.NewItems[0] == pvm2)
					changed = true;
			};

			isAvailable = true;

			// Bit of integration here, constrainting property changes will trigger availability requery
			pvm.Value = 5;

			Assert.That (changed, Is.True);
			Assert.That (vm.Properties, Contains.Item (pvm));
			Assert.That (vm.Properties, Contains.Item (pvm2));
		}

		[Test]
		public void Filtered ()
		{
			IObjectEditor editor = null;

			var prop = new Mock<IPropertyInfo> ();
			prop.SetupGet (p => p.Type).Returns (typeof(int));
			prop.SetupGet (p => p.Name).Returns ("one");
		
			var prop2 = new Mock<IPropertyInfo> ();
			prop2.SetupGet (p => p.Type).Returns (typeof(int));
			prop2.SetupGet (p => p.Name).Returns ("two");

			editor = new MockObjectEditor (prop.Object, prop2.Object);
			var pvm = new PropertyViewModel<int> (prop.Object, new[] { editor });
			var pvm2 = new PropertyViewModel<int> (prop2.Object, new[] { editor });

			var vm = new PropertyGroupViewModel ("category", new[] { pvm, pvm2 }, new [] { editor});
			Assume.That (vm.Properties, Contains.Item (pvm));
			Assume.That (vm.Properties, Contains.Item (pvm2));

			INotifyCollectionChanged notify = vm.Properties as INotifyCollectionChanged;
			Assume.That (notify, Is.Not.Null);

			bool changed = false;
			notify.CollectionChanged += (sender, args) => {
				if (args.Action == NotifyCollectionChangedAction.Remove && args.OldItems[0] == pvm)
					changed = true;
			};

			vm.FilterText = "t";

			Assert.That (changed, Is.True, "Collection changed event didn't trigger correctly");
			Assert.That (vm.Properties, Contains.Item (pvm2));
			Assert.That (vm.Properties, Does.Not.Contain (pvm));
			Assert.That (vm.HasChildElements, Is.True);
		}

		[Test]
		public void FilteredOutOfChildren ()
		{
			IObjectEditor editor = null;

			var prop = new Mock<IPropertyInfo> ();
			prop.SetupGet (p => p.Type).Returns (typeof(int));
			prop.SetupGet (p => p.Name).Returns ("one");
		
			var prop2 = new Mock<IPropertyInfo> ();
			prop2.SetupGet (p => p.Type).Returns (typeof(int));
			prop2.SetupGet (p => p.Name).Returns ("two");

			editor = new MockObjectEditor (prop.Object, prop2.Object);
			var pvm = new PropertyViewModel<int> (prop.Object, new[] { editor });
			var pvm2 = new PropertyViewModel<int> (prop2.Object, new[] { editor });

			var vm = new PropertyGroupViewModel ("category", new[] { pvm, pvm2 }, new [] { editor});
			Assume.That (vm.Properties, Contains.Item (pvm));
			Assume.That (vm.Properties, Contains.Item (pvm2));

			bool changed = false;
			vm.PropertyChanged += (sender, args) => {
				if (args.PropertyName == nameof(PropertyGroupViewModel.HasChildElements))
					changed = true;
			};

			vm.FilterText = "1";

			Assert.That (changed, Is.True, "HasChildElements didn't change");
			Assert.That (vm.HasChildElements, Is.False);
			Assert.That (vm.Properties, Does.Not.Contain (pvm2));
			Assert.That (vm.Properties, Does.Not.Contain (pvm));

			changed = false;
			vm.FilterText = null;

			Assert.That (changed, Is.True, "HasChildElements didn't change");
			Assert.That (vm.HasChildElements, Is.True);
			Assert.That (vm.Properties, Contains.Item (pvm2));
			Assert.That (vm.Properties, Contains.Item (pvm));
		}
	}
}
