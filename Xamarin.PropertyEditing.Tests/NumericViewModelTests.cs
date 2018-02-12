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
	internal abstract class NumericViewModelTests<T>
		: ConstrainedPropertyViewModelTests<T, NumericPropertyViewModel<T>>
		where T : struct, IComparable<T>
	{
		[Test]
		public void SelfConstrainedValuesCommands ()
		{
			T max, min;
			T value = GetConstrainedRandomValue (Random, out max, out min);
			Assume.That (max, Is.GreaterThan (min));
			Assume.That (value, Is.LessThan (max));
			Assume.That (value, Is.GreaterThan (min));

			var mockProperty = new Mock<IPropertyInfo> ();
			var constrainedMock = mockProperty.As<ISelfConstrainedPropertyInfo<T>> ();
			constrainedMock.SetupGet (pi => pi.MaxValue).Returns (max);
			constrainedMock.SetupGet (pi => pi.MinValue).Returns (min);

			var vm = GetViewModel (mockProperty.Object, new MockObjectEditor (mockProperty.Object));
			Assume.That (vm.MaximumValue, Is.EqualTo (max));
			Assume.That (vm.MinimumValue, Is.EqualTo (min));

			vm.Value = value;
			Assume.That (vm.Value, Is.EqualTo (value));

			Assume.That (vm.RaiseValue, Is.Not.Null);
			Assert.That (vm.RaiseValue.CanExecute (null), Is.True, "RaiseValue can not execute");
			Assert.That (vm.LowerValue.CanExecute (null), Is.True, "LowerValue can not execute");
		}

		[Test]
		public void SelfConstrainedBelowCommands ()
		{
			T max, min;
			T value = GetConstrainedRandomValueBelowBounds (Random, out max, out min);
			Assume.That (max, Is.GreaterThan (min));
			Assume.That (value, Is.LessThan (max));
			Assume.That (value, Is.LessThan (min));

			var mockProperty = new Mock<IPropertyInfo> ();
			var constrainedMock = mockProperty.As<ISelfConstrainedPropertyInfo<T>> ();
			constrainedMock.SetupGet (pi => pi.MaxValue).Returns (max);
			constrainedMock.SetupGet (pi => pi.MinValue).Returns (min);

			var vm = GetViewModel (mockProperty.Object, new MockObjectEditor (mockProperty.Object));

			vm.Value = value;
			Assume.That (vm.Value, Is.EqualTo (min));

			Assume.That (vm.RaiseValue, Is.Not.Null);
			Assert.That (vm.RaiseValue.CanExecute (null), Is.True, "Should be able to RaiseValue");
			Assert.That (vm.LowerValue.CanExecute (null), Is.False, "Should not be able to LowerValue");
		}

		[Test]
		public void SelfConstrainedAboveCommands ()
		{
			T max, min;
			T value = GetConstrainedRandomValueAboveBounds (Random, out max, out min);
			Assume.That (max, Is.GreaterThan (min));
			Assume.That (value, Is.GreaterThan (max));
			Assume.That (value, Is.GreaterThan (min));

			var mockProperty = new Mock<IPropertyInfo> ();
			var constrainedMock = mockProperty.As<ISelfConstrainedPropertyInfo<T>> ();
			constrainedMock.SetupGet (pi => pi.MaxValue).Returns (max);
			constrainedMock.SetupGet (pi => pi.MinValue).Returns (min);

			var vm = GetViewModel (mockProperty.Object, new MockObjectEditor (mockProperty.Object));

			vm.Value = value;
			Assume.That (vm.Value, Is.EqualTo (max));

			Assume.That (vm.RaiseValue, Is.Not.Null);
			Assert.That (vm.RaiseValue.CanExecute (null), Is.False, "Should not be able to RaiseValue");
			Assert.That (vm.LowerValue.CanExecute (null), Is.True, "Should be able to LowerValue");
		}
	}
}
