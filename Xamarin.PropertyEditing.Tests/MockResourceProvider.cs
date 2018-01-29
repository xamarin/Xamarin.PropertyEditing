using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Xamarin.PropertyEditing.Tests
{
	public class MockResourceProvider : IResourceProvider
	{
		public MockResourceProvider(IEnumerable<ResourceSource> sources, IEnumerable<Resource> resources)
		{
			if (sources == null) {
				throw new System.ArgumentNullException (nameof (sources));
			}
			if (resources == null) {
				throw new System.ArgumentNullException (nameof (resources));
			}

			this.sources = sources.ToArray ();
			this.resources = resources.ToList ();
		}

		public async Task<Resource> CreateResourceAsync<T> (ResourceSource source, string name, T value)
		{
			if (source == null) {
				throw new System.ArgumentNullException (nameof (source));
			}

			var newResource = new Resource<T> (source, name, value);
			this.resources.Add (newResource);
			return await Task.FromResult<Resource>(newResource);
		}

		public async Task<IReadOnlyList<Resource>> GetResourcesAsync (object target, IPropertyInfo property, CancellationToken cancelToken)
			=> await Task.FromResult ((IReadOnlyList<Resource>)(this.resources
				.Where (r => property.Type.IsAssignableFrom (r.RepresentationType))
				.ToArray ()));

		public async Task<IReadOnlyList<ResourceSource>> GetResourceSourcesAsync (object target, IPropertyInfo property)
			=> await Task.FromResult(this.sources);

		IReadOnlyList<ResourceSource> sources;
		IList<Resource> resources;
	}
}
