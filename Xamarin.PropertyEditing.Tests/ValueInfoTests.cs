using NUnit.Framework;

namespace Xamarin.PropertyEditing.Tests
{
	[TestFixture]
	internal class ValueInfoTests
	{
		[Test]
		public void ValueEquatable ()
		{
			var left = new ValueInfo<int> { Value = 5 };
			var right = new ValueInfo<int> { Value = 5 };

			AssertEqual (left, right);
		}

		[Test]
		public void SourceEquatable ()
		{
			var left = new ValueInfo<int> { Source = ValueSource.Binding };
			var right = new ValueInfo<int> { Source = ValueSource.Binding };

			AssertEqual (left, right);
		}

		[Test]
		public void DescriptorEquatable ()
		{
			string description = "monkeys";
			var left = new ValueInfo<int> { ValueDescriptor = description };
			var right = new ValueInfo<int> { ValueDescriptor = description };

			AssertEqual (left, right);
		}

		[Test]
		public void ValueButNotSourceUnequal ()
		{
			var left = new ValueInfo<int> {
				Value = 5,
				Source = ValueSource.Local
			};

			var right = new ValueInfo<int> {
				Value = 5,
				Source = ValueSource.Binding
			};

			AssertNotEqual (left, right);
		}

		private void AssertNotEqual<T> (ValueInfo<T> left, ValueInfo<T> right)
		{
			Assert.That (left, Is.Not.EqualTo (right));
			Assert.That (left != right);

			if (left != null)
				Assert.That (left.GetHashCode (), Is.Not.EqualTo (right.GetHashCode ()));
		}

		private void AssertEqual<T> (ValueInfo<T> left, ValueInfo<T> right)
		{
			Assert.That (left, Is.EqualTo (right));
			Assert.That (left == right);

			if (left != null)
				Assert.That (left.GetHashCode (), Is.EqualTo (right.GetHashCode ()));
		}
	}
}