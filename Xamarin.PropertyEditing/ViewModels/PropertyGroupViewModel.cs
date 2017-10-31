using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class PropertyGroupViewModel
		: NotifyingObject
	{
		public PropertyGroupViewModel (IEnumerable<PropertyViewModel> properties)
		{
			if (properties == null)
				throw new ArgumentNullException (nameof (properties));

			this.properties = properties.ToArray ();
			foreach (var vm in this.properties) {
				if (vm.IsAvailable)
					this.propertiesView.Add (vm);

				vm.PropertyChanged += OnChildPropertyChanged;
			}
		}

		public IEnumerable<PropertyViewModel> Properties => this.propertiesView;

		private readonly PropertyViewModel[] properties;
		private readonly ObservableCollectionEx<PropertyViewModel> propertiesView = new ObservableCollectionEx<PropertyViewModel> ();

		private void OnChildPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName != nameof(PropertyViewModel.IsAvailable))
				return;

			var vm = (PropertyViewModel) sender;
			if (vm.IsAvailable)
				this.propertiesView.Add (vm);
			else
				this.propertiesView.Remove (vm);
		}
	}
}