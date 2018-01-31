using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class ResourceSelectorViewModel
		: NotifyingObject
	{
		public ResourceSelectorViewModel (IResourceProvider provider, IEnumerable<object> targets, IPropertyInfo property)
		{
			if (provider == null)
				throw new ArgumentNullException (nameof (provider));
			if (targets == null)
				throw new ArgumentNullException (nameof (targets));
			if (property == null)
				throw new ArgumentNullException (nameof (property));

			Provider = provider;
			this.targets = targets.ToArray();
			Property = property;
			UpdateResources();

			this.resourcesView = new SimpleCollectionView (this.resources, new SimpleCollectionViewOptions {
				DisplaySelector = (o) => ((Resource)o).Name
			});
		}

		public IPropertyInfo Property
		{
			get;
		}

		public IResourceProvider Provider
		{
			get;
		}

		public IEnumerable Resources => this.resourcesView;

		public bool IsLoading
		{
			get { return this.isLoading; }
			set
			{
				if (this.isLoading == value)
					return;

				this.isLoading = value;
				OnPropertyChanged();
			}
		}

		public bool ShowSystemResources
		{
			get {  return this.showSystemResources; }
			set
			{
				if (this.showSystemResources == value)
					return;

				this.showSystemResources = value;
				OnPropertyChanged();
				UpdateResourceFilter();
			}
		}

		public string FilterText
		{
			get { return this.filterText; }
			set
			{
				if (this.filterText == value)
					return;

				this.filterText = value;
				OnPropertyChanged();
				UpdateResourceFilter();
			}
		}

		private readonly ObservableCollectionEx<Resource> resources = new ObservableCollectionEx<Resource>();
		private readonly SimpleCollectionView resourcesView;
		private bool showSystemResources = true, isLoading;
		private string filterText;
		private readonly object[] targets;

		private void UpdateResourceFilter()
		{
			if (String.IsNullOrWhiteSpace (FilterText) && ShowSystemResources) {
				this.resourcesView.Options.Filter = null;
				return;
			}

			this.resourcesView.Options.Filter = ResourceFilter;
		}

		private bool ResourceFilter (object item)
		{
			var r = (Resource)item;
			if (!String.IsNullOrWhiteSpace (FilterText) && !r.Name.StartsWith (FilterText, StringComparison.OrdinalIgnoreCase))
				return false;
			if (!ShowSystemResources && !r.Source.IsLocal)
				return false;

			return true;
		}

		private async void UpdateResources ()
		{
			await UpdateResourcesAsync();
		}

		private async Task UpdateResourcesAsync()
		{
			this.resources.Clear();

			if (Provider != null) {
				try {
					HashSet<Resource> joinedResources = null;
					var tasks = new HashSet<Task<IReadOnlyList<Resource>>> (this.targets.Select (t => Provider.GetResourcesAsync (t, Property, CancellationToken.None)));
					do {
						var task = await Task.WhenAny (tasks);
						tasks.Remove (task);

						if (joinedResources == null)
							joinedResources = new HashSet<Resource> (task.Result);
						else
							joinedResources.IntersectWith (task.Result);
					} while (tasks.Count > 0);

					this.resources.AddItems (joinedResources);
					IsLoading = false;
				} catch (OperationCanceledException) {
					return;
				}
			}
		}
	}
}
