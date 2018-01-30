using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Tests
{
	internal abstract class EnumPropertyViewModelTests<T>
		: PropertyViewModelTests<T, EnumPropertyViewModel<T>>
		where T : struct
	{
		[Test]
		public void IsFlags ()
		{
			var mockProperty = new Mock<IPropertyInfo> ();
			mockProperty.SetupGet (pi => pi.Type).Returns (typeof (T));
			var mockEditor = new Mock<IObjectEditor> ();

			var vm = new EnumPropertyViewModel<T> (TargetPlatform.Default, mockProperty.Object, new[] { mockEditor.Object });
			Assert.That (vm.IsFlags, Is.EqualTo (typeof(T).GetCustomAttribute<FlagsAttribute> () != null), "IsFlags was incorrect");
		}

		[Test]
		public void PossibleValues ()
		{
			var mockProperty = new Mock<IPropertyInfo> ();
			mockProperty.SetupGet (pi => pi.Type).Returns (typeof (T));
			var mockEditor = new Mock<IObjectEditor> ();

			var vm = new EnumPropertyViewModel<T> (TargetPlatform.Default, mockProperty.Object, new [] { mockEditor.Object });
			Assert.That (vm.PossibleValues, new CollectionEquivalentConstraint (Enum.GetNames (typeof(T))));
		}

		[Test]
		public void ValueName ()
		{
			var mockProperty = new Mock<IPropertyInfo> ();
			mockProperty.SetupGet (pi => pi.Type).Returns (typeof (T));
			var mockEditor = new Mock<IObjectEditor> ();

			T value = GetNonDefaultRandomTestValue ();
			Assume.That (value, Is.Not.EqualTo (default(T)));

			mockEditor.Setup (oe => oe.GetValueAsync<T> (mockProperty.Object, null)).Returns (Task.FromResult (new ValueInfo<T> { Source = ValueSource.Local, Value = value }));

			var vm = new EnumPropertyViewModel<T> (TargetPlatform.Default, mockProperty.Object, new[] { mockEditor.Object });
			Assume.That (vm.Value, Is.EqualTo (value));
			Assert.That (vm.ValueName, Is.EqualTo (value.ToString ()));
		}

		[Test]
		public async Task ValueNameChanged ()
		{
			var mockProperty = new Mock<IPropertyInfo> ();
			mockProperty.SetupGet (pi => pi.Type).Returns (typeof (T));

			T value = GetNonDefaultRandomTestValue ();
			Assume.That (value, Is.Not.EqualTo (default (T)));

			var editor = new MockObjectEditor (mockProperty.Object);
			await editor.SetValueAsync (mockProperty.Object, new ValueInfo<T> { Source = ValueSource.Local, Value = value });

			var vm = new EnumPropertyViewModel<T> (TargetPlatform.Default, mockProperty.Object, new[] { editor });
			Assume.That (vm.Value, Is.EqualTo (value));
			Assume.That (vm.ValueName, Is.EqualTo (value.ToString ()));

			T newValue = GetNonDefaultRandomTestValue ();
			while (Equals (newValue, value)) {
				newValue = GetNonDefaultRandomTestValue ();
			}

			bool changed = false;
			vm.PropertyChanged += (sender, args) => {
				if (args.PropertyName == nameof (EnumPropertyViewModel<T>.ValueName)) {
					changed = true;
				}
			};

			vm.Value = newValue;
			Assert.That (changed, Is.True);
			Assert.That (vm.ValueName, Is.EqualTo (newValue.ToString ()));
		}
	}

	internal enum TestEnum
	{
		None = 0,
		First = 1,
		Second = 2,
		Third = 3,
		Fourth = 4
	}

	[TestFixture]
	internal class EnumPropertyViewModelTests
		: EnumPropertyViewModelTests<TestEnum>
	{
		protected override TestEnum GetRandomTestValue (Random rand)
			=> rand.Next<TestEnum> ();

		protected override EnumPropertyViewModel<TestEnum> GetViewModel (TargetPlatform platform, IPropertyInfo property, IEnumerable<IObjectEditor> editors)
		{
			return new EnumPropertyViewModel<TestEnum> (platform, property, editors);
		}
	}

	[Flags]
	internal enum FlagsTestEnum
	{
		None = 0,
		Flag1 = 1,
		Flag2 = 2,
		Flag3 = 4,
		Flag4 = 8
	}

	[TestFixture]
	internal class FlagsEnumPropertyViewModelTests
		: EnumPropertyViewModelTests<FlagsTestEnum>
	{
		protected override FlagsTestEnum GetRandomTestValue (Random rand)
		{
			FlagsTestEnum[] values = (FlagsTestEnum[])Enum.GetValues (typeof (FlagsTestEnum));
			int index = rand.Next (0, values.Length - 1);

			FlagsTestEnum value = values[index];
			if (index > 0) {
				int flags = rand.Next (0, values.Length - 2);
				for (int i = 0; i < flags; i++) {
					FlagsTestEnum rflag;
					do {
						rflag = values[rand.Next (1, values.Length - 1)];
					} while (value.HasFlag (rflag));

					value |= rflag;
				}
			}

			return value;
		}

		protected override EnumPropertyViewModel<FlagsTestEnum> GetViewModel (TargetPlatform platform, IPropertyInfo property, IEnumerable<IObjectEditor> editors)
		{
			return new EnumPropertyViewModel<FlagsTestEnum> (platform, property, editors);
		}
	}
}
