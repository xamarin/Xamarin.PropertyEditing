using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Xamarin.PropertyEditing.Tests
{
	[TestFixture]
	internal class ResourceTests
	{
		[Test]
		public void NameEquality ()
		{
			const string name = "name";
			const string name2 = "other";
			var r = new Resource (name);
			var r2 = new Resource (name);
			var r3 = new Resource (name2);

			Assert.That (r, Is.EqualTo (r2));
			Assert.That (r, Is.Not.EqualTo (r3));
		}

		[Test]
		public void SourceEquality ()
		{
			const string name = "name";
			const string sourceName = "source";
			var source = new ResourceSource (sourceName, isLocal: true);
			var source2 = new ResourceSource (sourceName, isLocal: false);
			Assume.That (source, Is.Not.EqualTo (source2));

			var r = new Resource (source, name);
			var r2 = new Resource (source, name);
			var r3 = new Resource (name);
			var r4 = new Resource (source2, name);
			var r5 = new Resource (source, sourceName);

			Assert.That (r, Is.EqualTo (r2));
			Assert.That (r, Is.Not.EqualTo (r3));
			Assert.That (r, Is.Not.EqualTo (r4));
			Assert.That (r, Is.Not.EqualTo (r5));
		}

		[Test]
		public void GetHashCodeSource ()
		{
			const string name = "name";
			const string sourceName = "source";
			var source = new ResourceSource (sourceName, isLocal: true);
			var source2 = new ResourceSource (sourceName, isLocal: false);
			Assume.That (source.GetHashCode(), Is.Not.EqualTo (source2.GetHashCode()));

			var r = new Resource (source, name);
			var r2 = new Resource (source, name);
			var r3 = new Resource (name);
			var r4 = new Resource (source2, name);
			var r5 = new Resource (source, sourceName);

			Assert.That (r.GetHashCode(), Is.EqualTo (r2.GetHashCode()));
			Assert.That (r.GetHashCode(), Is.Not.EqualTo (r3.GetHashCode()));
			Assert.That (r.GetHashCode(), Is.Not.EqualTo (r4.GetHashCode()));
			Assert.That (r.GetHashCode(), Is.Not.EqualTo (r5.GetHashCode()));
		}
	}
}
