using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.Tests
{
	public class MockResourceProvider
		: IResourceProvider
	{
		public Task<Resource> CreateResourceAsync<T> (ResourceSource source, string name, T value)
		{
			throw new NotImplementedException ();
		}

		public Task<IReadOnlyList<Resource>> GetResourcesAsync (object target, IPropertyInfo property, CancellationToken cancelToken)
		{
			return Task.FromResult<IReadOnlyList<Resource>> (SystemResources.Where (r => property.Type.IsAssignableFrom (r.GetType().GetGenericArguments()[0])).ToList());
		}

		public Task<IReadOnlyList<ResourceSource>> GetResourceSourcesAsync (object target, IPropertyInfo property)
		{
			return Task.FromResult<IReadOnlyList<ResourceSource>> (new[] { SystemResourcesSource });
		}

		internal static readonly ResourceSource SystemResourcesSource = new ResourceSource ("System Resources", isLocal: false);

		private static readonly IReadOnlyList<Resource> SystemResources = new Resource[] {
			new Resource<CommonSolidBrush> (SystemResourcesSource, "ControlTextBrush", new CommonSolidBrush (0, 0, 0)),
			new Resource<CommonSolidBrush> (SystemResourcesSource, "HighlightBrush", new CommonSolidBrush (51, 153, 255)),
			new Resource<CommonSolidBrush> (SystemResourcesSource, "TransparentBrush", new CommonSolidBrush (0, 0, 0, 0)),
			new Resource<CommonColor> (SystemResourcesSource, "ControlTextColor", new CommonColor (0, 0, 0)),
			new Resource<CommonColor> (SystemResourcesSource, "HighlightColor", new CommonColor (51, 153, 255))
		};
	}
}
