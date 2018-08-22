using System;
using System.Collections.Generic;
using AppKit;
using Foundation;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class ResourceDataSource : NSOutlineViewDataSource
	{
		public ResourceDataSource (ResourceSelectorViewModel viewModel) : base ()
		{
			this.vm = viewModel;
		}

		public override nint GetChildrenCount (NSOutlineView outlineView, NSObject item)
		{
			if (this.vm?.Resources == null)
				return 0;

			if (this.vm.Resources?.Count () == 0)
				return 0;

			return this.vm.Resources.Count ();
		}

		public override NSObject GetChild (NSOutlineView outlineView, nint childIndex, NSObject item)
		{
			object element;

			element = this.vm.Resources[(int)childIndex];

			return GetFacade (element);
		}

		public override bool ItemExpandable (NSOutlineView outlineView, NSObject item)
		{
			return false;
		}

		public NSObject GetFacade (object element)
		{
			NSObject facade;

			if (!this.groupFacades.TryGetValue (element, out facade)) {
				this.groupFacades[element] = facade = new NSObjectFacade (element);
			}
			return facade;
		}

		public bool TryGetFacade (object element, out NSObject facade)
		{
			return this.groupFacades.TryGetValue (element, out facade);
		}

		private readonly ResourceSelectorViewModel vm;
		private readonly Dictionary<object, NSObject> groupFacades = new Dictionary<object, NSObject> ();
	}
}
