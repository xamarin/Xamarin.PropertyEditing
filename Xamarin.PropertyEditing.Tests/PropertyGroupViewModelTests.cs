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

			var vm = new PropertyGroupViewModel (new[] { pvm, pvm2 });
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

			var vm = new PropertyGroupViewModel (new[] { pvm, pvm2 });
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

			var vm = new PropertyGroupViewModel (new[] { pvm, pvm2 });
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
	}
}
