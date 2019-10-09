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
			provider.ResourcesChanged += OnResourcesChanged;

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

		public IList Resources => this.resourcesView;

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

		public bool ShowOnlySystemResources
		{
			get {  return this.showOnlySystemResources; }
			set
			{
				if (!SetOnlySystemResources (value))
					return;

				SetOnlyLocalResources (false);
				SetBothResourceTypes (false);
				UpdateResourceFilter();
			}
		}

		public bool ShowBothResourceTypes
		{
			get {  return this.showBothResourceTypes; }
			set
			{
				if (!SetBothResourceTypes (value))
					return;

				SetOnlyLocalResources (false);
				SetOnlySystemResources (false);
				UpdateResourceFilter();
			}
		}

		public bool ShowOnlyLocalResources
		{
			get {  return this.showOnlyLocalResources; }
			set
			{
				if (!SetOnlyLocalResources (value))
					return;

				SetOnlySystemResources (false);
				SetBothResourceTypes (false);
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
		private bool showOnlySystemResources = false, showOnlyLocalResources = false, showBothResourceTypes = true, isLoading;
		private string filterText;
		private readonly object[] targets;

		private bool SetOnlyLocalResources (bool value)
		{
			if (this.showOnlyLocalResources == value)
				return false;

			this.showOnlyLocalResources = value;
			OnPropertyChanged();
			return true;
		}

		private bool SetOnlySystemResources (bool value)
		{
			if (this.showOnlySystemResources == value)
				return false;

			this.showOnlySystemResources = value;
			OnPropertyChanged();
			return true;
		}

		private bool SetBothResourceTypes (bool value)
		{
			if (this.showBothResourceTypes == value)
				return false;

			this.showBothResourceTypes = value;
			OnPropertyChanged();
			return true;
		}

		private void UpdateResourceFilter()
		{
			if (String.IsNullOrWhiteSpace (FilterText) && ShowBothResourceTypes) {
				this.resourcesView.Options.Filter = null;
				return;
			}

			this.resourcesView.Options.Filter = ResourceFilter;
		}

		private bool ResourceFilter (object item)
		{
			var r = (Resource)item;
			if (!String.IsNullOrWhiteSpace (FilterText) && !r.Name.Contains (FilterText, StringComparison.OrdinalIgnoreCase))
				return false;
			if (ShowOnlySystemResources && r.Source.Type != ResourceSourceType.System)
				return false;
			if (ShowOnlyLocalResources && r.Source.Type == ResourceSourceType.System)
				return false;

			return true;
		}

		private void OnResourcesChanged (object sender, EventArgs e)
		{
			UpdateResources ();
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

						if (task.Result == null || task.Result.Count == 0)
							continue;

						if (joinedResources == null)
							joinedResources = new HashSet<Resource> (task.Result);
						else
							joinedResources.IntersectWith (task.Result);
					} while (tasks.Count > 0);

					if (joinedResources != null)
						this.resources.AddItems (joinedResources);

					IsLoading = false;
				} catch (OperationCanceledException) {
					return;
				}
			}
		}
	}
}
