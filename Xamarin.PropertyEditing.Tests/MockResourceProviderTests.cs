using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Xamarin.PropertyEditing.Tests.MockPropertyInfo;

namespace Xamarin.PropertyEditing.Tests
{
	[TestFixture]
	public class MockResourceProviderTests
	{
		[Test]
		public async Task GetResourcesByPropertyType()
		{
			var source = new ResourceSource ("Source", true);

			var stringResource1 = new Resource<string> (source, "StringResource1", "One");
			var stringResource2 = new Resource<string> (source, "StringResource2", "Two");
			var intResource = new Resource<int> (source, "IntResource1", 1);

			var resourceProvider = new MockResourceProvider (
				new ResourceSource[] { source },
				new Resource[] { stringResource1, intResource, stringResource2 });

			IReadOnlyList<Resource> strings = await resourceProvider.GetResourcesAsync (null, new MockPropertyInfo<string> ("StringProperty"), new CancellationToken());
			IReadOnlyList<Resource> ints = await resourceProvider.GetResourcesAsync (null, new MockPropertyInfo<int> ("IntProperty"), new CancellationToken ());

			Assert.That (strings, Is.EquivalentTo (new[] { stringResource1, stringResource2 }));
			Assert.That (ints, Is.EquivalentTo (new[] { intResource }));
		}

		[Test]
		public async Task GetResourcesByBaseType ()
		{
			var source = new ResourceSource ("Source", true);

			var resourceA = new Resource<A> (source, "ResourceA", new A());
			var resourceB = new Resource<B> (source, "ResourceB", new B());

			var resourceProvider = new MockResourceProvider (
				new ResourceSource[] { source },
				new Resource[] { resourceA, resourceB });

			IReadOnlyList<Resource> resources = await resourceProvider.GetResourcesAsync (null, new MockPropertyInfo<A> ("AProperty"), new CancellationToken ());

			Assert.That (resources, Is.EquivalentTo (new IResource<A>[] { resourceA, resourceB }));
		}

		[Test]
		public async Task CreateResourceAddsNewResources()
		{
			var source = new ResourceSource ("Source", true);

			var stringResource1 = new Resource<string> (source, "StringResource1", "One");

			var resourceProvider = new MockResourceProvider (
				new ResourceSource[] { source },
				new Resource[] { stringResource1 });

			var addedResource = await resourceProvider.CreateResourceAsync (source, "StringResource2", "Two") as Resource<string>;

			IReadOnlyList<Resource> strings = await resourceProvider.GetResourcesAsync (null, new MockPropertyInfo<string> ("StringProperty"), new CancellationToken ());

			Assert.That (addedResource, Is.Not.Null);
			Assert.That (addedResource.Name, Is.EqualTo ("StringResource2"));
			Assert.That (addedResource.Value, Is.EqualTo ("Two"));
			Assert.That (addedResource.Source, Is.EqualTo (source));
			Assert.That (strings, Is.EquivalentTo (new[] { stringResource1, addedResource }));
		}

		[Test]
		public async Task GetResourceSources()
		{
			var source1 = new ResourceSource ("Source1", true);
			var source2 = new ResourceSource ("Source2", true);

			var resourceProvider = new MockResourceProvider (
				new ResourceSource[] { source1, source2 },
				new Resource[] { });

			var sources = await resourceProvider.GetResourceSourcesAsync (null, null);

			Assert.That (sources, Is.EquivalentTo (new ResourceSource[] { source1, source2 }));
		}

		private class A { }
		private class B : A { }
	}
}
