using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using AppKit;
using Foundation;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class PropertyTableDataSource
		: NSOutlineViewDataSource
	{
		bool Filtering => !string.IsNullOrEmpty (this.vm.FilterText);

		internal PropertyTableDataSource (PanelViewModel panelVm)
		{
			if (panelVm == null)
				throw new ArgumentNullException (nameof (panelVm));

			this.vm = panelVm;
		}

		public PanelViewModel DataContext => this.vm;

		public override nint GetChildrenCount (NSOutlineView outlineView, NSObject item)
		{
			if (this.vm.ArrangedEditors.Count == 0)
				return 0;

			var facade = (NSObjectFacade)item;
			if (facade?.Target is ObjectPropertyViewModel ovm)
				return ovm.ValueModel.Properties.Count;

			var childCount = 0;
			if (this.vm.ArrangeMode == PropertyArrangeMode.Name)
				childCount = Filtering ? this.vm.ArrangedEditors[0].Editors.Count : this.vm.ArrangedEditors[0].Editors.Count + 1;
			else {
				if (item == null)
					childCount = Filtering ? this.vm.ArrangedEditors.Count : this.vm.ArrangedEditors.Count + 1;
				else {
					var group = (PanelGroupViewModel)((NSObjectFacade)item).Target;
					childCount = group.Editors.Count + group.UncommonEditors.Count;
				}
			}

			return childCount;
		}

		public override NSObject GetChild (NSOutlineView outlineView, nint childIndex, NSObject item)
		{
			object element;

			var f = ((NSObjectFacade)item);
			// We only want the Header to appear at the top of both Category and Name Modes, which means item is null in both.
			if (childIndex == 0 && item == null && !Filtering)
				element = null;
			else if (f?.Target is ObjectPropertyViewModel ovm) {
				element = ovm.ValueModel.Properties[(int)childIndex];
			} else {
				if (this.vm.ArrangeMode == PropertyArrangeMode.Name)
					element = Filtering ? this.vm.ArrangedEditors[0].Editors[(int)childIndex] : this.vm.ArrangedEditors[0].Editors[(int)childIndex - 1];
				else {
					if (item == null)
						element = Filtering ? this.vm.ArrangedEditors[(int)childIndex] : this.vm.ArrangedEditors[(int)childIndex - 1];
					else {
						var group = (PanelGroupViewModel)f.Target;
						var list = group.Editors;
						if (childIndex >= list.Count) {
							childIndex -= list.Count;
							list = group.UncommonEditors;
						}

						element = list[(int)childIndex];
					}
				}
			}

			return GetFacade (element);
		}

		public override bool ItemExpandable (NSOutlineView outlineView, NSObject item)
		{
			var f = (NSObjectFacade)item;
			if (f.Target is ObjectPropertyViewModel ovm) {
				PropertyChangedEventHandler changed = null;
				changed = (o, e) => {
					if (e.PropertyName != nameof (ObjectPropertyViewModel.CanDelve))
						return;

					ovm.PropertyChanged -= changed;
					outlineView.ReloadItem (item);
				};
				ovm.PropertyChanged += changed;

				return ovm.CanDelve;
			}

			if (this.vm.ArrangeMode == PropertyArrangeMode.Name)
				return false;

			return f.Target is PanelGroupViewModel;
		}

		public NSObject GetFacade (object element)
		{
			NSObject facade;
			if (element is PanelGroupViewModel) {
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
