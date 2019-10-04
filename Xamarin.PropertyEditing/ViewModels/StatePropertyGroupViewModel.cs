using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class StatePropertyGroupViewModel
		: PropertyGroupViewModel, IPropertyHost
	{
		public StatePropertyGroupViewModel (TargetPlatform platform, PropertyViewModel parentProperty, IEnumerable<PropertyViewModel> properties, IEnumerable<IObjectEditor> objEditors)
			: base (platform, parentProperty?.Name, properties, objEditors)
		{
			if (parentProperty == null)
				throw new ArgumentNullException (nameof(parentProperty));

			HostedProperty = parentProperty;
		}

		public override string Name => HostedProperty.Name;

		public override string Category => HostedProperty.Category;

		public override bool CanDelve => this.panel != null && this.panel.ArrangeMode != PropertyArrangeMode.Name;

		public override bool DelveByDefault => true;

		public PropertyViewModel HostedProperty
		{
			get;
		}

		protected override void OnPropertyChanged (string propertyName = null)
		{
			if (propertyName == nameof(Parent)) {
				if (this.panel != null) {
					this.panel.PropertyChanged -= PanelOnPropertyChanged;
				}

				this.panel = Parent as PanelViewModel;
				if (this.panel != null) {
					this.panel.PropertyChanged += PanelOnPropertyChanged;
				}

				OnPropertyChanged (nameof(PanelViewModel));
			}

			base.OnPropertyChanged (propertyName);
		}

		private PanelViewModel panel;

		private void PanelOnPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(PanelViewModel.ArrangeMode))
				OnPropertyChanged (nameof(CanDelve));
		}
	}
}
