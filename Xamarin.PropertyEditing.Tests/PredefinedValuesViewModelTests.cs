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
	internal abstract class PredefinedValuesViewModelTests<T>
		: PropertyViewModelTests<T, PredefinedValuesViewModel<T>>
	{
		[Test]
		public void NameMatches ()
		{
			T testValue = GetRandomTestValue ();
			string name = GetName (testValue);

			var vm = GetBasicTestModel (testValue);
			Assert.That (vm.ValueName, Is.EqualTo (name));
		}

		[Test]
		public void NewValueNameMatches ()
		{
			T testValue = GetNonDefaultRandomTestValue ();
			string testValueName = GetName (testValue);

			var vm = GetBasicTestModel ();
			Assume.That (vm.Value, Is.Not.EqualTo (testValue));
			Assume.That (vm.ValueName, Is.Not.EqualTo (testValueName));

			vm.Value = testValue;
			Assert.That (vm.ValueName, Is.EqualTo (testValueName));
		}

		[Test]
		public void NewValueChangesValueNameProperty ()
		{
			T testValue = GetNonDefaultRandomTestValue ();

			var vm = GetBasicTestModel ();
			Assume.That (vm.Value, Is.Not.EqualTo (testValue));

			bool changed = false;
			vm.PropertyChanged += (sender, args) => {
				if (args.PropertyName == nameof(vm.ValueName))
					changed = true;
			};

			vm.Value = testValue;
			Assert.That (changed, Is.True);
		}

		[Test]
		public void NewValueNameValueMatches ()
		{
			T testValue = GetNonDefaultRandomTestValue ();
			string testValueName = GetName (testValue);

			var vm = GetBasicTestModel ();
			Assume.That (vm.Value, Is.Not.EqualTo (testValue));
			Assume.That (vm.ValueName, Is.Not.EqualTo (testValueName));

			vm.ValueName = testValueName;
			Assert.That (vm.Value, Is.EqualTo (testValue));
		}

		[Test]
		public void NewValueNameChangesValue ()
		{
			T testValue = GetNonDefaultRandomTestValue ();
			string testValueName = GetName (testValue);

			var vm = GetBasicTestModel ();
			Assume.That (vm.Value, Is.Not.EqualTo (testValue));
			Assume.That (vm.ValueName, Is.Not.EqualTo (testValueName));

			bool changed = false;
			vm.PropertyChanged += (sender, args) => {
				if (args.PropertyName == nameof(vm.Value))
					changed = true;
			};

			vm.ValueName = testValueName;
			Assert.That (changed, Is.True);
		}

		[Test]
		public void DuplicateValues ()
		{
			T testValue = GetNonDefaultRandomTestValue ();

			var property = GetPropertyMock ();
			var predefined = property.As<IHavePredefinedValues<T>> ();
			predefined.SetupGet (p => p.PredefinedValues).Returns (new Dictionary<string, T> {
				{ "Value", testValue },
				{ "SameValue", testValue }
			});

			var vm = GetViewModel (property.Object, new[] { GetBasicEditor (property.Object) });

			Assume.That (vm.ValueName, Is.Not.EqualTo ("Value"));
			vm.ValueName = "Value";
			Assert.That (vm.Value, Is.EqualTo (testValue));
			
			bool changed = false;
			vm.PropertyChanged += (sender, args) => {
				if (args.PropertyName == nameof(vm.Value))
					changed = true;
			};

			vm.ValueName = "SameValue";
			Assert.That (vm.Value, Is.EqualTo (testValue));
			Assert.That (changed, Is.False);
		}

		protected abstract bool IsConstrained { get; }
		protected abstract IReadOnlyDictionary<string, T> Values { get; }

		protected string GetName (T value)
		{
			string name = GetNameOrDefault (value);
			Assume.That (name, Is.Not.Null);
			return name;
		}

		protected string GetNameOrDefault (T value)
		{
			return Values.Where (kvp => Equals (value, kvp.Value)).Select (kvp => kvp.Key).FirstOrDefault ();
		}

		protected override void AugmentPropertyMock (Mock<IPropertyInfo> propertyMock)
		{
			var predefined = propertyMock.As<IHavePredefinedValues<T>> ();
			predefined.SetupGet (h => h.IsConstrainedToPredefined).Returns (IsConstrained);
			predefined.SetupGet (h => h.PredefinedValues).Returns (Values);
		}
	}

	internal abstract class ConstrainedPredefinedValuesViewModelTests<T>
		: PredefinedValuesViewModelTests<T>
	{
		protected override bool IsConstrained => true;

		protected abstract T GetOutOfBoundsValue ();

		protected abstract string GetOutOfBoundsValueName ();

		protected abstract string GetRandomValueName (Random rand);

		protected string GetRandomValueName ()
		{
			return GetRandomValueName (Random);
		}

		[Test]
		public void SetValueOutOfBoundsWithDefault ()
		{
			T value = GetOutOfBoundsValue ();

			var property = GetPropertyMock ();
			property.SetupGet (pi => pi.ValueSources).Returns (ValueSources.Default | ValueSources.Local);

			var editor = GetBasicEditor (property.Object);

			var vm = GetViewModel (property.Object, new[] { editor });
			T originalValue = vm.Value;
			string originalValueName = vm.ValueName;
			Assume.That (vm.Value, Is.Not.EqualTo (value));

			vm.Value = value;
			Assert.That (vm.Value, Is.EqualTo (originalValue));
			Assert.That (vm.ValueName, Is.EqualTo (originalValueName));
		}

		[Test]
		public void SetValueNameOutOfBoundsWithDefault ()
		{
			string valueName = GetOutOfBoundsValueName ();

			var property = GetPropertyMock ();
			property.SetupGet (pi => pi.ValueSources).Returns (ValueSources.Default | ValueSources.Local);

			var editor = GetBasicEditor (property.Object);

			var vm = GetViewModel (property.Object, new[] { editor });
			T originalValue = vm.Value;
			string originalValueName = vm.ValueName;
			Assume.That (vm.ValueName, Is.EqualTo (GetName (default(T))));

			bool changed = false;
			vm.PropertyChanged += (sender, args) => {
				if (args.PropertyName == nameof(vm.ValueName))
					changed = true;
			};

			vm.ValueName = valueName;
			Assert.That (vm.Value, Is.EqualTo (originalValue));
			Assert.That (vm.ValueName, Is.EqualTo (originalValueName));
			Assert.That (changed, Is.False);
		}

		[Test]
		public void UnsetSource ()
		{
			var property = GetPropertyMock ();
			property.SetupGet (pi => pi.ValueSources).Returns (ValueSources.Local);

			var editor = new Mock<IObjectEditor> ();
			editor.SetupGet (o => o.Target).Returns (new object ());
			editor.SetupGet (oe => oe.Properties).Returns (new[] { property.Object });
			editor.Setup (oe => oe.GetValueAsync<T> (property.Object, null)).ReturnsAsync (new ValueInfo<T> {
				Value = default(T),
				Source = ValueSource.Unset
			});

			var vm = GetViewModel (property.Object, new[] { editor.Object });
			Assert.That (vm.ValueName, Is.EqualTo (String.Empty));
			Assert.That (vm.ValueSource, Is.EqualTo (ValueSource.Unset));
		}

		[Test]
		[Description ("If we're constrained but can't use a default, we need to include a blank selection for unset")]
		public void UnsetIncludesBlank ()
		{
			var property = GetPropertyMock ();
			property.SetupGet (pi => pi.ValueSources).Returns (ValueSources.Local);

			var editor = GetBasicEditor (property.Object);

			var vm = GetViewModel (property.Object, new[] { editor });
			Assume.That (vm.ValueName, Is.EqualTo (String.Empty));

			Assert.That (vm.PossibleValues, Contains.Item (String.Empty));
		}

		[Test]
		[Description ("When passing along strings in the value desciprtor, we should ensure they are empty and not null for differentiation")]
		public void ValueDescriptorEmptyNotNullConstrained ()
		{
			var property = GetPropertyMock ();
			property.SetupGet (pi => pi.ValueSources).Returns (ValueSources.Local);

			var editor = GetBasicEditor (property.Object);

			var vm = GetViewModel (property.Object, new[] { editor });
			Assume.That (vm.ValueName, Is.EqualTo (String.Empty));

			vm.ValueName = GetRandomValueName();
			vm.ValueName = null;

			var info = editor.values[property.Object] as ValueInfo<T>;
			Assert.That (info, Is.Not.Null);
			Assert.That (info.ValueDescriptor, Is.EqualTo (String.Empty));
			Assert.That (info.Source, Is.EqualTo (ValueSource.Local));
		}
	}

	internal enum PredefinedEnumTest
		: int
	{
		None = 0,
		First = 1,
		Second = 2,
		Eigth = 8
	}

	[TestFixture]
	internal class UnconstrainedPredefinedViewModelTests
		: PredefinedValuesViewModelTests<string>
	{
		[Test]
		public void ValueDescriptorForUnconstrained ()
		{
			var property = GetPropertyMock ();
			var predefined = property.As<IHavePredefinedValues<string>> ();
			predefined.SetupGet (p => p.PredefinedValues).Returns (new Dictionary<string, string> {
				{ "Value", GetNonDefaultRandomTestValue () },
			});
			predefined.SetupGet (p => p.IsConstrainedToPredefined).Returns (false);

			var editor = GetBasicEditor (property.Object);

			var vm = GetViewModel (property.Object, new[] { editor });
			Assume.That (vm.ValueName, Is.EqualTo (String.Empty));

			vm.ValueName = "test";

			var info = editor.values[property.Object] as ValueInfo<string>;
			Assert.That (info, Is.Not.Null);
			Assert.That (info.ValueDescriptor, Is.EqualTo ("test"));
			Assert.That (info.Source, Is.EqualTo (ValueSource.Local));
		}

		[Test]
		[Description ("When passing along strings in the value desciprtor, we should ensure they are empty and not null for differentiation")]
		public void ValueDescriptorEmptyNotNullUnconstrained ()
		{
			var property = GetPropertyMock ();
			var predefined = property.As<IHavePredefinedValues<string>> ();
			predefined.SetupGet (p => p.PredefinedValues).Returns (new Dictionary<string, string> {
				{ "Value", GetNonDefaultRandomTestValue () },
			});
			predefined.SetupGet (p => p.IsConstrainedToPredefined).Returns (false);

			var editor = GetBasicEditor (property.Object);

			var vm = GetViewModel (property.Object, new[] { editor });
			Assume.That (vm.ValueName, Is.EqualTo (String.Empty));

			vm.ValueName = "test";
			vm.ValueName = null;

			var info = editor.values[property.Object] as ValueInfo<string>;
			Assert.That (info, Is.Not.Null);
			Assert.That (info.ValueDescriptor, Is.EqualTo (String.Empty));
			Assert.That (info.Source, Is.EqualTo (ValueSource.Local));
		}

		protected override string GetRandomTestValue (Random rand)
		{
			string[] names = Enum.GetNames (typeof(PredefinedEnumTest));
			int index = rand.Next (0, names.Length);
			return names[index];
		}

		protected override PredefinedValuesViewModel<string> GetViewModel (TargetPlatform platform, IPropertyInfo property, IEnumerable<IObjectEditor> editors)
		{
			return new PredefinedValuesViewModel<string> (platform, property, editors);
		}

		protected override bool IsConstrained => false;

		protected override IReadOnlyDictionary<string, string> Values =>
			Enum.GetNames (typeof(PredefinedEnumTest)).ToDictionary (s => s, s => s);
	}

	[TestFixture]
	internal class EnumPredefinedViewModelTests
		: ConstrainedPredefinedValuesViewModelTests<int>
	{
		public EnumPredefinedViewModelTests ()
		{
			this.values = (int[]) Enum.GetValues (typeof(PredefinedEnumTest));
			this.names = Enum.GetNames (typeof(PredefinedEnumTest));

			var v = new Dictionary<string, int> (this.values.Length);
			for (int i = 0; i < this.values.Length; i++) {
				v.Add (this.names[i], this.values[i]);
			}

			this.predefinedValues = v;
		}

		protected override IReadOnlyDictionary<string, int> Values => this.predefinedValues;

		protected override string GetOutOfBoundsValueName ()
		{
			return "foo";
		}

		protected override int GetOutOfBoundsValue ()
		{
			return -1;
		}

		protected override int GetRandomTestValue (Random rand)
		{
			int index = rand.Next (0, this.values.Length);
			return this.values[index];
		}

		protected override string GetRandomValueName (Random rand)
		{
			int index = rand.Next (0, this.values.Length);
			return this.names[index];
		}

		protected override PredefinedValuesViewModel<int> GetViewModel (TargetPlatform platform, IPropertyInfo property, IEnumerable<IObjectEditor> editors)
		{
			return new PredefinedValuesViewModel<int> (platform, property, editors);
		}

		private readonly IReadOnlyDictionary<string, int> predefinedValues;
		private readonly int[] values;
		private readonly string[] names;
	}
}
