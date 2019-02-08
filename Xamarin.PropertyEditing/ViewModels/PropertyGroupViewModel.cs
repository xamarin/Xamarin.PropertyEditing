using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class PropertyGroupViewModel
		: EditorViewModel, IFilterable
	{
		public PropertyGroupViewModel (TargetPlatform platform, string category, IEnumerable<PropertyViewModel> properties, IEnumerable<IObjectEditor> objEditors)
			: base (platform, objEditors)
		{
			if (category == null)
				throw new ArgumentNullException (nameof(category));
			if (properties == null)
				throw new ArgumentNullException (nameof (properties));

			Category = category;

			this.properties = properties.ToList ();
			foreach (var vm in this.properties) {
				if (vm.IsAvailable)
					this.propertiesView.Add (vm);

				vm.PropertyChanged += OnChildPropertyChanged;
			}
		}

		public IReadOnlyList<PropertyViewModel> Properties => this.propertiesView;

		public override string Name => null;

		public override string Category
		{
			get;
		}

		public bool HasChildElements => (this.propertiesView.Count > 0);

		public string FilterText
		{
			get { return this.filterText; }
			set
			{
				if (this.filterText == value)
					return;

				string oldFilter = this.filterText;
				this.filterText = value;
				Filter (oldFilter);
				OnPropertyChanged();
			}
		}

		public void Add (PropertyViewModel property)
		{
			if (property == null)
				throw new ArgumentNullException (nameof(property));

			this.properties.Add (property);
			Reload ();
		}

		public bool Remove (PropertyViewModel property)
		{
			if (property == null)
				throw new ArgumentNullException (nameof(property));

			if (this.properties.Remove (property))
				return this.propertiesView.Remove (property);

			return false;
		}

		private readonly List<PropertyViewModel> properties;
		private readonly ObservableCollectionEx<PropertyViewModel> propertiesView = new ObservableCollectionEx<PropertyViewModel> ();
		private string filterText;

		private void Reload ()
		{
			Filter (null);
		}

		private void Filter (string oldFilter)
		{
			bool hadChildren = HasChildElements;

			if (FilterText != null && (String.IsNullOrWhiteSpace (oldFilter) || FilterText.StartsWith (oldFilter, StringComparison.OrdinalIgnoreCase))) {
				var current = new List<PropertyViewModel> (this.propertiesView);
				for (int i = 0; i < current.Count; i++) {
					var vm = current[i];
					if (!MatchesFilter (vm))
						this.propertiesView.Remove (vm);
				}
			} else {
				this.propertiesView.Reset (this.properties.Where (MatchesFilter).OrderBy (p => p.Property.Name));
			}

			if (hadChildren != HasChildElements)
				OnPropertyChanged (nameof(HasChildElements));
		}

		private bool MatchesFilter (PropertyViewModel vm)
		{
			if (!vm.IsAvailable)
				return false;
			if (String.IsNullOrWhiteSpace (FilterText))
				return true;

			return (vm.Property.Name.Contains (FilterText, StringComparison.OrdinalIgnoreCase));
		}

		private void OnChildPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName != nameof(PropertyViewModel.IsAvailable))
				return;

			var vm = (PropertyViewModel) sender;
			if (MatchesFilter (vm))
				this.propertiesView.Add (vm);
			else
				this.propertiesView.Remove (vm);
		}
	}
}