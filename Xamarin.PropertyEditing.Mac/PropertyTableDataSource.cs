using System;
using System.Collections.Generic;
using System.Linq;
using AppKit;
using Foundation;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class PropertyTableDataSource
		: NSOutlineViewDataSource
	{
		internal PropertyTableDataSource (PanelViewModel panelVm)
		{
			if (panelVm == null)
				throw new ArgumentNullException (nameof (panelVm));

			this.vm = panelVm;
		}

		public PanelViewModel DataContext => this.vm;

		public override nint GetChildrenCount (NSOutlineView outlineView, NSObject item)
		{
			if (this.vm.ArrangedProperties.Count == 0)
				return 0;

			if (this.vm.ArrangeMode == PropertyArrangeMode.Name)
				return this.vm.ArrangedProperties[0].Count;

			if (item == null)
				return this.vm.ArrangedProperties.Count;
			else
				return ((IGroupingList<string, PropertyViewModel>)((NSObjectFacade)item).Target).Count;
		}

		public override NSObject GetChild (NSOutlineView outlineView, nint childIndex, NSObject item)
		{
			object element;
			if (this.vm.ArrangeMode == PropertyArrangeMode.Name) {
				element = (this.vm.ArrangedProperties[0][(int)childIndex]);
			} else {
				if (item == null)
					element = this.vm.ArrangedProperties[(int)childIndex];
				else {
					element = ((IGroupingList<string, PropertyViewModel>)((NSObjectFacade)item).Target)[(int)childIndex];
				}
			}

			return GetFacade (element);
		}

		public override bool ItemExpandable (NSOutlineView outlineView, NSObject item)
		{
			if (this.vm.ArrangeMode == PropertyArrangeMode.Name)
				return false;

			return ((NSObjectFacade)item).Target is IGroupingList<string, PropertyViewModel>;
		}

		public NSObject GetFacade (object element)
		{
			NSObject facade;
			if (element is IGrouping<string, PropertyViewModel>) {
				if (!this.groupFacades.TryGetValue (element, out facade)) {
					this.groupFacades[element] = facade = new NSObjectFacade (element);
				}
			} else
				facade = new NSObjectFacade (element);

			return facade;
		}

		public bool TryGetFacade (object element, out NSObject facade)
		{
			return this.groupFacades.TryGetValue (element, out facade);
		}

		private readonly PanelViewModel vm;
		private readonly Dictionary<object, NSObject> groupFacades = new Dictionary<object, NSObject> ();
	}
}
