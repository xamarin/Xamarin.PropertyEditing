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

			var childCount = 0;

			if (this.vm.ArrangeMode == PropertyArrangeMode.Name)
				childCount = Filtering ? this.vm.ArrangedEditors[0].Count : this.vm.ArrangedEditors[0].Count + 1;
			else {
				if (item == null)
					childCount = Filtering ? this.vm.ArrangedEditors.Count : this.vm.ArrangedEditors.Count + 1;
				else
					childCount = ((IGroupingList<string, EditorViewModel>)((NSObjectFacade)item).Target).Count;
			}

			return childCount;
		}

		public override NSObject GetChild (NSOutlineView outlineView, nint childIndex, NSObject item)
		{
			object element;

			// We only want the Header to appear at the top of both Category and Name Modes, which means item is null in both.
			if (childIndex == 0 && item == null && !Filtering)
				element = new PanelHeaderEditorControl (this.vm);
			else {
				if (this.vm.ArrangeMode == PropertyArrangeMode.Name)
					element = Filtering ? this.vm.ArrangedEditors[0][(int)childIndex] : this.vm.ArrangedEditors[0][(int)childIndex - 1];
				else {
					if (item == null)
						element = Filtering ? this.vm.ArrangedEditors[(int)childIndex] : this.vm.ArrangedEditors[(int)childIndex - 1];
					else {
						element = ((IGroupingList<string, EditorViewModel>)((NSObjectFacade)item).Target)[(int)childIndex];
					}
				}
			}

			return GetFacade (element);
		}

		public override bool ItemExpandable (NSOutlineView outlineView, NSObject item)
		{
			if (this.vm.ArrangeMode == PropertyArrangeMode.Name)
				return false;

			return ((NSObjectFacade)item).Target is IGroupingList<string, EditorViewModel>;
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
