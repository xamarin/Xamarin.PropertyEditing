using System;
using Moq;
using NUnit.Framework;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Tests
{
	internal abstract class ConstrainedPropertyViewModelTests<T>
		: PropertyViewModelTests<T>
		where T : IComparable<T>
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

			var vm = GetViewModel (mockProperty.Object, new Mock<IObjectEditor> ().Object);
			Assert.That (vm.MaximumValue, Is.EqualTo (max));
			Assert.That (vm.MinimumValue, Is.EqualTo (min));

			vm.Value = value;
			Assume.That (vm.Value, Is.EqualTo (value));

			Assume.That (vm.RaiseValue, Is.Not.Null);
			Assert.That (vm.RaiseValue.CanExecute (null), Is.True, "RaiseValue can not execute");
			Assert.That (vm.LowerValue.CanExecute (null), Is.True, "LowerValue can not execute");
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

			var vm = GetViewModel (mockProperty.Object, new Mock<IObjectEditor> ().Object);

			vm.Value = value;
			Assert.That (vm.Value, Is.EqualTo (min));

			Assume.That (vm.RaiseValue, Is.Not.Null);
			Assert.That (vm.RaiseValue.CanExecute (null), Is.True, "Should be able to RaiseValue");
			Assert.That (vm.LowerValue.CanExecute (null), Is.False, "Should not be able to LowerValue");
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

			var vm = GetViewModel (mockProperty.Object, new Mock<IObjectEditor> ().Object);

			vm.Value = value;
			Assert.That (vm.Value, Is.EqualTo (max));

			Assume.That (vm.RaiseValue, Is.Not.Null);
			Assert.That (vm.RaiseValue.CanExecute (null), Is.False, "Should not be able to RaiseValue");
			Assert.That (vm.LowerValue.CanExecute (null), Is.True, "Should be able to LowerValue");
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
	        mockEditor.Setup (oe => oe.GetValue<T> (mockMaxProperty.Object, null)).Returns (new ValueInfo<T> { Value = max, Source = ValueSource.Local });
            mockEditor.Setup (oe => oe.GetValue<T> (mockMinProperty.Object, null)).Returns (new ValueInfo<T> { Value = min, Source = ValueSource.Local });

	        var vm = GetViewModel (mockValueProperty.Object, mockEditor.Object);
	        Assert.That (vm.MinimumValue, Is.EqualTo (min));
	        Assert.That (vm.MaximumValue, Is.EqualTo (max));
	    }

		[Test]
		public void UnconstrainedMaxMinAtTypeMaxMin ()
		{
			T max = MaxMin.Item1;
			T min = MaxMin.Item2;

			var mockValueProperty = new Mock<IPropertyInfo> ();
			var mockEditor = new Mock<IObjectEditor> ();

			var vm = GetViewModel (mockValueProperty.Object, mockEditor.Object);
			Assert.That (vm.IsConstrained, Is.False, "Reporting constrained with no constraint implementations");
			Assert.That (vm.MaximumValue, Is.EqualTo (max));
			Assert.That (vm.MinimumValue, Is.EqualTo (min));
		}

		[Test]
		public void UnconstrainedMaxMinAtTypeMaxMinAfterChange ()
		{
			T max = MaxMin.Item1;
			T min = MaxMin.Item2;

			var mockValueProperty = new Mock<IPropertyInfo> ();
			var mockEditor = new Mock<IObjectEditor> ();

			var vm = GetViewModel (mockValueProperty.Object, mockEditor.Object);
			Assume.That (vm.IsConstrained, Is.False, "Reporting constrained with no constraint implementations");
			Assume.That (vm.MaximumValue, Is.EqualTo (max));
			Assume.That (vm.MinimumValue, Is.EqualTo (min));

			mockEditor.Raise (oe => oe.PropertyChanged += null, new EditorPropertyChangedEventArgs (null));
			Assert.That (vm.MaximumValue, Is.EqualTo (max));
			Assert.That (vm.MinimumValue, Is.EqualTo (min));
		}


		protected abstract Tuple<T,T> MaxMin { get; }

		protected abstract T GetConstrainedRandomValue (Random rand, out T max, out T min);

		protected abstract T GetConstrainedRandomValueAboveBounds (Random rand, out T max, out T min);
		protected abstract T GetConstrainedRandomValueBelowBounds (Random rand, out T max, out T min);

		protected ConstrainedPropertyViewModel<T> GetViewModel (IPropertyInfo property, IObjectEditor editor)
	    {
	        return (ConstrainedPropertyViewModel<T>) GetViewModel (property, new[] { editor });
	    }
	}
}
