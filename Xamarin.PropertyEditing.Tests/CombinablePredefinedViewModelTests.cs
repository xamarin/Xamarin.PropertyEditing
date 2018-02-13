using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Tests
{
	[TestFixture]
	internal class CombinablePredefinedViewModelTests
		: PropertyViewModelTests<int, CombinablePropertyViewModel<int>>
	{
		[Test]
		public void GetsFlags ()
		{
			FlagsTestEnum value = FlagsTestEnum.Flag2 | FlagsTestEnum.Flag3;

			var p = GetPropertyMock ();

			var target = new object();
			var editorMock = new Mock<IObjectEditor> ();
			editorMock.SetupGet (e => e.Target).Returns (target);
			editorMock.Setup (e => e.GetValueAsync<IReadOnlyList<int>> (p.Object, null)).ReturnsAsync (
				new ValueInfo<IReadOnlyList<int>> {
					Value = new int[] { (int) FlagsTestEnum.Flag2, (int) FlagsTestEnum.Flag3 },
					Source = ValueSource.Local
				});
			editorMock.Setup (e => e.GetValueAsync<int> (p.Object, null)).ReturnsAsync (new ValueInfo<int> {
				Value = (int)value,
				Source = ValueSource.Local
			});

			var vm = GetViewModel (p.Object, editorMock.Object);
			Assume.That (vm.Choices.Count, Is.EqualTo (7));

			var flag2Choice = vm.Choices.Single (v => v.Name == nameof(FlagsTestEnum.Flag2));
			Assume.That (flag2Choice.Value, Is.EqualTo ((int)FlagsTestEnum.Flag2));
			var flag3Choice = vm.Choices.Single (v => v.Name == nameof(FlagsTestEnum.Flag3));
			Assume.That (flag3Choice.Value, Is.EqualTo ((int)FlagsTestEnum.Flag3));

			Assert.That (vm.Value, Is.EqualTo ((int)value));
			Assert.That (flag2Choice.IsFlagged.HasValue && flag2Choice.IsFlagged.Value, Is.True);
			Assert.That (flag3Choice.IsFlagged.HasValue && flag3Choice.IsFlagged.Value, Is.True);
			Assert.That (vm.Choices.Count (v => v.IsFlagged.HasValue && !v.IsFlagged.Value), Is.EqualTo (5), "Unselected choices not marked false");
		}

		[Test]
		public void GetsFlagsDisasgree ()
		{
			FlagsTestEnum value = FlagsTestEnum.Flag2 | FlagsTestEnum.Flag3;
			FlagsTestEnum value2 = FlagsTestEnum.Flag2 | FlagsTestEnum.Flag4;

			var p = GetPropertyMock ();

			var target = new object();
			var editorMock = new Mock<IObjectEditor> ();
			editorMock.SetupGet (e => e.Target).Returns (target);
			editorMock.Setup (e => e.GetValueAsync<IReadOnlyList<int>> (p.Object, null)).ReturnsAsync (
				new ValueInfo<IReadOnlyList<int>> {
					Value = new int[] { (int) FlagsTestEnum.Flag2, (int) FlagsTestEnum.Flag3 },
					Source = ValueSource.Local
				});
			editorMock.Setup (e => e.GetValueAsync<int> (p.Object, null)).ReturnsAsync (new ValueInfo<int> {
				Value = (int)value,
				Source = ValueSource.Local
			});

			var target2 = new object();
			var editorMock2 = new Mock<IObjectEditor> ();
			editorMock2.SetupGet (e => e.Target).Returns (target2);
			editorMock2.Setup (e => e.GetValueAsync<IReadOnlyList<int>> (p.Object, null)).ReturnsAsync (
				new ValueInfo<IReadOnlyList<int>> {
					Value = new int[] { (int) FlagsTestEnum.Flag2, (int) FlagsTestEnum.Flag4 },
					Source = ValueSource.Local
				});
			editorMock2.Setup (e => e.GetValueAsync<int> (p.Object, null)).ReturnsAsync (new ValueInfo<int> {
				Value = (int)value2,
				Source = ValueSource.Local
			});

			var vm = GetViewModel (p.Object, new[] { editorMock.Object, editorMock2.Object });
			Assume.That (vm.Choices.Count, Is.EqualTo (7));

			var flag2Choice = vm.Choices.Single (v => v.Name == nameof(FlagsTestEnum.Flag2));
			Assume.That (flag2Choice.Value, Is.EqualTo ((int)FlagsTestEnum.Flag2));
			var flag3Choice = vm.Choices.Single (v => v.Name == nameof(FlagsTestEnum.Flag3));
			Assume.That (flag3Choice.Value, Is.EqualTo ((int)FlagsTestEnum.Flag3));
			var flag4Choice = vm.Choices.Single (v => v.Name == nameof(FlagsTestEnum.Flag4));
			Assume.That (flag4Choice.Value, Is.EqualTo ((int)FlagsTestEnum.Flag4));
			var flag5Choice = vm.Choices.Single (v => v.Name == nameof(FlagsTestEnum.Flag5));
			Assume.That (flag5Choice.Value, Is.EqualTo ((int)FlagsTestEnum.Flag5));

			Assert.That (vm.Value, Is.EqualTo (default(int)));
			Assert.That (vm.MultipleValues, Is.True, "MultipleValues is not marked true for disagreeing values");
			Assert.That (flag2Choice.IsFlagged.HasValue && flag2Choice.IsFlagged.Value, Is.True);
			Assert.That (flag3Choice.IsFlagged.HasValue, Is.False, "Disagreed upon choice is not indeterminate");
			Assert.That (flag4Choice.IsFlagged.HasValue, Is.False, "Disagreed upon choice is not indeterminate");
			Assert.That (flag5Choice.IsFlagged.HasValue, Is.False, "Disagreed upon duplicate choice is not indeterminate");
			Assert.That (vm.Choices.Count (v => v.IsFlagged.HasValue && !v.IsFlagged.Value), Is.EqualTo (3), "Unselected choices not marked false");
		}

		[Test]
		public void SetFlags ()
		{
			FlagsTestEnum value = FlagsTestEnum.Flag2 | FlagsTestEnum.Flag3;

			var p = GetPropertyMock ();

			var target = new object();
			var editorMock = new Mock<IObjectEditor> ();
			editorMock.SetupGet (e => e.Target).Returns (target);
			editorMock.Setup (e => e.GetValueAsync<IReadOnlyList<int>> (p.Object, null)).ReturnsAsync (
				new ValueInfo<IReadOnlyList<int>> {
					Value = new int[] { (int) FlagsTestEnum.Flag2, (int) FlagsTestEnum.Flag3 },
					Source = ValueSource.Local
				});
			editorMock.Setup (e => e.GetValueAsync<int> (p.Object, null)).ReturnsAsync (new ValueInfo<int> {
				Value = (int)value,
				Source = ValueSource.Local
			});

			ValueInfo<IReadOnlyList<int>> setValue = null;
			editorMock.Setup (oe => oe.SetValueAsync (p.Object, It.IsAny<ValueInfo<IReadOnlyList<int>>> (), null))
				.Callback<IPropertyInfo, ValueInfo<IReadOnlyList<int>>, PropertyVariation> ((pi, v, variation) => {
					setValue = v;
					editorMock.Setup (e => e.GetValueAsync<IReadOnlyList<int>> (p.Object, null)).ReturnsAsync (v);

					int rv = 0;
					for (int i = 0; i < v.Value.Count; i++)
						rv |= v.Value[i];

					editorMock.Setup (e => e.GetValueAsync<int> (p.Object, null)).ReturnsAsync (new ValueInfo<int> {
						Value = rv,
						Source = ValueSource.Local
					});
					editorMock.Raise (oe => oe.PropertyChanged += null, new EditorPropertyChangedEventArgs (p.Object));
				});

			var vm = GetViewModel (p.Object, editorMock.Object);
			Assume.That (vm.Choices.Count, Is.EqualTo (7));
			Assume.That (vm.Value, Is.EqualTo ((int) value));

			value |= FlagsTestEnum.Flag1;
			vm.Choices[1].IsFlagged = true;
			Assert.That (setValue, Is.Not.Null, "Did not call setvalue");
			CollectionAssert.AreEquivalent (new[] { (int)FlagsTestEnum.Flag1, (int)FlagsTestEnum.Flag2, (int)FlagsTestEnum.Flag3 }, setValue.Value);
			Assert.That (vm.Value, Is.EqualTo ((int) value));
		}


		[Test]
		public void SetFlagsFromUnset ()
		{
			var p = GetPropertyMock ();

			var target = new object();
			var editorMock = new Mock<IObjectEditor> ();
			editorMock.SetupGet (e => e.Target).Returns (target);
			editorMock.Setup (e => e.GetValueAsync<IReadOnlyList<int>> (p.Object, null)).ReturnsAsync (
				new ValueInfo<IReadOnlyList<int>> {
					Value = null,
					Source = ValueSource.Unset
				});
			editorMock.Setup (e => e.GetValueAsync<int> (p.Object, null)).ReturnsAsync (new ValueInfo<int> {
				Value = default(int),
				Source = ValueSource.Unset
			});

			ValueInfo<IReadOnlyList<int>> setValue = null;
			editorMock.Setup (oe => oe.SetValueAsync (p.Object, It.IsAny<ValueInfo<IReadOnlyList<int>>> (), null))
				.Callback<IPropertyInfo, ValueInfo<IReadOnlyList<int>>, PropertyVariation> ((pi, v, variation) => {
					setValue = v;
					editorMock.Setup (e => e.GetValueAsync<IReadOnlyList<int>> (p.Object, null)).ReturnsAsync (v);

					int rv = 0;
					for (int i = 0; i < v.Value.Count; i++)
						rv |= v.Value[i];

					editorMock.Setup (e => e.GetValueAsync<int> (p.Object, null)).ReturnsAsync (new ValueInfo<int> {
						Value = rv,
						Source = ValueSource.Local
					});
					editorMock.Raise (oe => oe.PropertyChanged += null, new EditorPropertyChangedEventArgs (p.Object));
				});

			FlagsTestEnum value = FlagsTestEnum.Flag1;

			var vm = GetViewModel (p.Object, editorMock.Object);
			Assume.That (vm.Choices.Count, Is.EqualTo (7));
			Assume.That (vm.Value, Is.EqualTo (default(int)));
		
			vm.Choices[1].IsFlagged = true;
			Assert.That (setValue, Is.Not.Null, "Did not call setvalue");
			CollectionAssert.AreEquivalent (new[] { (int)FlagsTestEnum.Flag1 }, setValue.Value);
			Assert.That (vm.Value, Is.EqualTo ((int) value));
		}

		[Test]
		[Ignore ("There's either a bug in the test or in Moq, but the callbacks seem to be causing NREs even empty")]
		public void SetFlagsMultipleValues ()
		{
			FlagsTestEnum value = FlagsTestEnum.Flag2 | FlagsTestEnum.Flag3;
			FlagsTestEnum value2 = FlagsTestEnum.Flag2 | FlagsTestEnum.Flag4;

			var p = GetPropertyMock ();

			var target = new object();
			var editorMock = new Mock<IObjectEditor> ();
			editorMock.SetupGet (e => e.Target).Returns (target);
			editorMock.Setup (e => e.GetValueAsync<IReadOnlyList<int>> (p.Object, null)).ReturnsAsync (
				new ValueInfo<IReadOnlyList<int>> {
					Value = new int[] { (int) FlagsTestEnum.Flag2, (int) FlagsTestEnum.Flag3 },
					Source = ValueSource.Local
				});
			editorMock.Setup (e => e.GetValueAsync<int> (p.Object, null)).ReturnsAsync (new ValueInfo<int> {
				Value = (int)value,
				Source = ValueSource.Local
			});

			var target2 = new object();
			var editorMock2 = new Mock<IObjectEditor> ();
			editorMock2.SetupGet (e => e.Target).Returns (target2);
			editorMock2.Setup (e => e.GetValueAsync<IReadOnlyList<int>> (p.Object, null)).ReturnsAsync (
				new ValueInfo<IReadOnlyList<int>> {
					Value = new int[] { (int) FlagsTestEnum.Flag2, (int) FlagsTestEnum.Flag4 },
					Source = ValueSource.Local
				});
			editorMock2.Setup (e => e.GetValueAsync<int> (p.Object, null)).ReturnsAsync (new ValueInfo<int> {
				Value = (int)value2,
				Source = ValueSource.Local
			});

			ValueInfo<IReadOnlyList<int>> setValue = null;
			editorMock.Setup (oe => oe.SetValueAsync (p.Object, It.IsAny<ValueInfo<IReadOnlyList<int>>> (), null))
				.Callback<IPropertyInfo, ValueInfo<IReadOnlyList<int>>, PropertyVariation> ((pi, v, variation) => {
					setValue = v;
					editorMock.Setup (e => e.GetValueAsync<IReadOnlyList<int>> (p.Object, null)).ReturnsAsync (v);

					int rv = 0;
					for (int i = 0; i < v.Value.Count; i++)
						rv |= v.Value[i];

					editorMock.Setup (e => e.GetValueAsync<int> (p.Object, null)).ReturnsAsync (new ValueInfo<int> {
						Value = rv,
						Source = ValueSource.Local
					});
					editorMock.Raise (oe => oe.PropertyChanged += null, new EditorPropertyChangedEventArgs (p.Object));
				});

			ValueInfo<IReadOnlyList<int>> setValue2 = null;
			editorMock2.Setup (oe => oe.SetValueAsync (p.Object, It.IsAny<ValueInfo<IReadOnlyList<int>>> (), null))
				.Callback<IPropertyInfo, ValueInfo<IReadOnlyList<int>>, PropertyVariation> ((pi, v, variation) => {
					setValue2 = v;
					editorMock2.Setup (e => e.GetValueAsync<IReadOnlyList<int>> (p.Object, null)).ReturnsAsync (v);

					int rv = 0;
					for (int i = 0; i < v.Value.Count; i++)
						rv |= v.Value[i];

					editorMock2.Setup (e => e.GetValueAsync<int> (p.Object, null)).ReturnsAsync (new ValueInfo<int> {
						Value = rv,
						Source = ValueSource.Local
					});
					editorMock2.Raise (oe => oe.PropertyChanged += null, new EditorPropertyChangedEventArgs (p.Object));
				});

			var vm = GetViewModel (p.Object, new [] { editorMock.Object, editorMock2.Object });
			Assume.That (vm.Choices.Count, Is.EqualTo (7));
			Assume.That (vm.Value, Is.EqualTo (default(int)));

			var flag1Choice = vm.Choices.Single (v => v.Name == nameof(FlagsTestEnum.Flag1));
			Assume.That (flag1Choice.Value, Is.EqualTo ((int)FlagsTestEnum.Flag1));
			Assume.That (flag1Choice.IsFlagged.HasValue && !flag1Choice.IsFlagged.Value);
			var flag2Choice = vm.Choices.Single (v => v.Name == nameof(FlagsTestEnum.Flag2));
			Assume.That (flag2Choice.Value, Is.EqualTo ((int)FlagsTestEnum.Flag2));
			Assume.That (flag2Choice.IsFlagged.HasValue && flag2Choice.IsFlagged.Value, Is.True);
			var flag3Choice = vm.Choices.Single (v => v.Name == nameof(FlagsTestEnum.Flag3));
			Assume.That (flag3Choice.Value, Is.EqualTo ((int)FlagsTestEnum.Flag3));
			Assume.That (flag3Choice.IsFlagged.HasValue, Is.False);
			var flag4Choice = vm.Choices.Single (v => v.Name == nameof(FlagsTestEnum.Flag4));
			Assume.That (flag4Choice.Value, Is.EqualTo ((int)FlagsTestEnum.Flag4));
			Assume.That (flag4Choice.IsFlagged.HasValue, Is.False);
			var flag5Choice = vm.Choices.Single (v => v.Name == nameof(FlagsTestEnum.Flag5));
			Assume.That (flag5Choice.Value, Is.EqualTo ((int)FlagsTestEnum.Flag5));
			Assume.That (flag5Choice.IsFlagged.HasValue, Is.False);

			vm.Choices[1].IsFlagged = true;

			Assert.That (setValue, Is.Not.Null, "Did not call setvalue");
			CollectionAssert.AreEquivalent (new[] { (int)FlagsTestEnum.Flag1, (int)FlagsTestEnum.Flag2, (int)FlagsTestEnum.Flag3 }, setValue.Value);
			Assert.That (vm.Value, Is.EqualTo (default(int)));

			Assert.That (setValue2, Is.Not.Null, "Did not call setvalue");
			CollectionAssert.AreEquivalent (new[] { (int)FlagsTestEnum.Flag1, (int)FlagsTestEnum.Flag2, (int)FlagsTestEnum.Flag4, (int)FlagsTestEnum.Flag5 }, setValue.Value);
		}

		[Flags]

		internal enum FlagsTestEnum
		{
			None = 0,
			Flag1 = 1,
			Flag2 = 2,
			Flag3 = 4,
			Flag4 = 8,
			Flag5 = 8,
			All = Flag1 | Flag2 | Flag3 | Flag4
		}

		protected override void AugmentPropertyMock (Mock<IPropertyInfo> propertyMock)
		{
			base.AugmentPropertyMock (propertyMock);

			var values = Enum.GetValues (typeof(FlagsTestEnum));
			var keys = Enum.GetNames (typeof(FlagsTestEnum));
			var dict = new Dictionary<string, int> ();
			for (int i = 0; i < keys.Length; i++) {
				dict[keys[i]] = ((int[]) values)[i];
			}

			var predefined = propertyMock.As<IHavePredefinedValues<int>> ();
			predefined.SetupGet (pv => pv.PredefinedValues).Returns (dict);
			predefined.SetupGet (pv => pv.IsValueCombinable).Returns (true);
			predefined.SetupGet (pv => pv.IsConstrainedToPredefined).Returns (true);
		}

		protected override int GetRandomTestValue (Random rand)
		{
			return ((int[])Enum.GetValues (typeof(FlagsTestEnum)))[rand.Next (0, 5)];
		}

		protected override CombinablePropertyViewModel<int> GetViewModel (TargetPlatform platform, IPropertyInfo property, IEnumerable<IObjectEditor> editors)
		{
			return new CombinablePropertyViewModel<int> (platform, property, editors);
		}
	}
}
