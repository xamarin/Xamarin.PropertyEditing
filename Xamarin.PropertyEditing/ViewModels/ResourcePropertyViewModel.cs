// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class ResourcePropertyViewModel
		: PropertyViewModel<Resource>
	{
		public ResourcePropertyViewModel (TargetPlatform platform, IPropertyInfo property, IEnumerable<IObjectEditor> editors, PropertyVariation variation = null)
			: base (platform, property, editors, variation)
		{
			UpdateResourceSelector();
		}

		public ResourceSelectorViewModel Selector
		{
			get { return this.selector; }
			private set
			{
				if (this.selector == value)
					return;

				this.selector = value;
				OnPropertyChanged();
			}
		}

		protected override void OnEditorsChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			base.OnEditorsChanged (sender, e);
			UpdateResourceSelector();
		}

		private ResourceSelectorViewModel selector;

		private void UpdateResourceSelector ()
		{
			if (Property == null)
				return;

			Selector = new ResourceSelectorViewModel (TargetPlatform.ResourceProvider, Editors.Select (oe => oe.Target), Property);
		}
	}
}
