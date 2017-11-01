using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Tests
{
	internal abstract class ConstrainedPropertyViewModelTests<T, TReal, TViewModel>
		: PropertyViewModelTests<T, TReal, TViewModel>
		where TReal : IComparable<TReal>
		where TViewModel : ConstrainedPropertyViewModel<T>
	{
		[Test]
		public void SelfConstrainedValues ()
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
			Assert.That (vm.MaximumValue, Is.EqualTo (max));
			Assert.That (vm.MinimumValue, Is.EqualTo (min));

			vm.Value = value;
			Assume.That (vm.Value, Is.EqualTo (value));
		}

		[Test]
		public void SelfConstrainedBelow ()
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
			Assert.That (vm.Value, Is.EqualTo (min));
		}

		[Test]
		public void SelfConstrainedAbove ()
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
			Assert.That (vm.Value, Is.EqualTo (max));
		}

		[Test]
		public void PropertyClamped ()
		{
			T max, min;
			T value = GetConstrainedRandomValue (Random, out max, out min);
			Assume.That (max, Is.GreaterThan (min));
			Assume.That (value, Is.LessThan (max));
			Assume.That (value, Is.GreaterThan (min));

			var mockMaxProperty = new Mock<IPropertyInfo> ();
			var mockMinProperty = new Mock<IPropertyInfo> ();

			var mockValueProperty = new Mock<IPropertyInfo> ();
			var mockConstrainedValueProperty = mockValueProperty.As<IClampedPropertyInfo> ();
			mockConstrainedValueProperty.SetupGet (pi => pi.MaximumProperty).Returns (mockMaxProperty.Object);
			mockConstrainedValueProperty.SetupGet (pi => pi.MinimumProperty).Returns (mockMinProperty.Object);

			var mockEditor = new Mock<IObjectEditor> ();
			mockEditor.Setup (oe => oe.GetValueAsync<T> (mockMaxProperty.Object, null)).Returns (Task.FromResult (new ValueInfo<T> { Value = max, Source = ValueSource.Local }));
			mockEditor.Setup (oe => oe.GetValueAsync<T> (mockMinProperty.Object, null)).Returns (Task.FromResult (new ValueInfo<T> { Value = min, Source = ValueSource.Local }));

			var vm = GetViewModel (mockValueProperty.Object, mockEditor.Object);
			Assert.That (vm.MinimumValue, Is.EqualTo (min));
			Assert.That (vm.MaximumValue, Is.EqualTo (max));
		}

		protected abstract Tuple<T, T> MaxMin
		{
			get;
		}

		protected abstract T GetConstrainedRandomValue (Random rand, out T max, out T min);

		protected abstract T GetConstrainedRandomValueAboveBounds (Random rand, out T max, out T min);
		protected abstract T GetConstrainedRandomValueBelowBounds (Random rand, out T max, out T min);
	}
}
