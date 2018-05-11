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
		public bool CanCreateResources => true;

		public Task<ResourceCreateError> CheckNameErrorsAsync (object target, ResourceSource source, string name)
		{
			ResourceCreateError error = null;
			if (this.resources[source].Any (r => r.Name == name)) {
				error = new ResourceCreateError ("Name in use", isWarning: false);
			} else {
				var order = new List<ResourceSourceType> {
					ResourceSourceType.Document,
					ResourceSourceType.ResourceDictionary,
					ResourceSourceType.Application,
					ResourceSourceType.System,
				};

				// Simplistic example of hierarchy override checking
				for (int i = order.IndexOf (source.Type)+1; i < order.Count; i++) {
					if (this.resources.Where (ig => ig.Key.Type == order[i]).SelectMany (ig => ig).Any (r => r.Name == name)) {
						error = new ResourceCreateError ("Resource would override another resource", isWarning: true);
						break;
					}
				}
			}

			return Task.FromResult (error);
		}

		public Task<Resource> CreateResourceAsync<T> (ResourceSource source, string name, T value)
		{
			var r = new Resource<T> (source, name, value);
			((ObservableLookup<ResourceSource, Resource>)this.resources).Add (source, r);
			return Task.FromResult<Resource> (r);
		}

		public Task<IReadOnlyList<Resource>> GetResourcesAsync (object target, IPropertyInfo property, CancellationToken cancelToken)
		{
			return Task.FromResult<IReadOnlyList<Resource>> (this.resources.SelectMany (g => g).Where (r => property.Type.IsAssignableFrom (r.GetType().GetGenericArguments()[0])).ToList());
		}

		public Task<IReadOnlyList<ResourceSource>> GetResourceSourcesAsync (object target, IPropertyInfo property)
		{
			return Task.FromResult<IReadOnlyList<ResourceSource>> (this.resources.Select (g => g.Key).ToArray ());
		}

		public Task<string> SuggestResourceNameAsync (IReadOnlyCollection<object> targets, IPropertyInfo property)
		{
			int i = 1;
			string key;
			do {
				key = property.Type.Name + i++;
			} while (this.resources[ApplicationResourcesSource].Any (r => r.Name == key));

			return Task.FromResult (key);
		}

		internal static readonly ResourceSource SystemResourcesSource = new ResourceSource ("System Resources", ResourceSourceType.System);
		private static readonly ResourceSource ApplicationResourcesSource = new ResourceSource ("App resources", ResourceSourceType.Application);

		private readonly ILookup<ResourceSource, Resource> resources = new ObservableLookup<ResourceSource, Resource> {
			new ObservableGrouping<ResourceSource, Resource> (SystemResourcesSource) {
				new Resource<CommonSolidBrush> (SystemResourcesSource, "ControlTextBrush", new CommonSolidBrush (0, 0, 0)),
				new Resource<CommonSolidBrush> (SystemResourcesSource, "HighlightBrush", new CommonSolidBrush (51, 153, 255)),
				new Resource<CommonSolidBrush> (SystemResourcesSource, "TransparentBrush", new CommonSolidBrush (0, 0, 0, 0)),
				new Resource<CommonColor> (SystemResourcesSource, "ControlTextColor", new CommonColor (0, 0, 0)),
				new Resource<CommonColor> (SystemResourcesSource, "HighlightColor", new CommonColor (51, 153, 255))
			},

			new ObservableGrouping<ResourceSource, Resource> (ApplicationResourcesSource) {
				new Resource<CommonSolidBrush> (SystemResourcesSource, "CustomHighlightBrush", new CommonSolidBrush (255, 165, 0)),
			}
		};
	}
}
