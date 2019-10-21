﻿using System;
using System.ComponentModel;
using AppKit;
using Foundation;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class PropertyTableDataSource
		: BaseOutlineViewDataSource
	{
		internal PropertyTableDataSource (PanelViewModel panelVm) : base(panelVm)
		{
		}

		public bool ShowHeader
		{
			get;
			set;
		} = true;

		public override nint GetChildrenCount (NSOutlineView outlineView, NSObject item)
		{
			if (DataContext.ArrangedEditors.Count == 0)
				return 0;

			var facade = (NSObjectFacade)item;
			if (facade?.Target is ObjectPropertyViewModel ovm)
				return ovm.ValueModel.Properties.Count;
				
			int headerCount = (ShowHeader && !Filtering) ? 1 : 0;

			if (DataContext.ArrangeMode == PropertyArrangeMode.Name)
				return DataContext.ArrangedEditors[0].Editors.Count + headerCount;
			else {
				if (item == null)
					return DataContext.ArrangedEditors.Count + headerCount;
				else {
					var group = (PanelGroupViewModel)((NSObjectFacade)item).Target;
					return group.Editors.Count + group.UncommonEditors.Count;
				}
			}
		}

		public override NSObject GetChild (NSOutlineView outlineView, nint childIndex, NSObject item)
		{
			object element;

			var f = ((NSObjectFacade)item);
			// We only want the Header to appear at the top of both Category and Name Modes, which means item is null in both.
			if (childIndex == 0 && item == null && !Filtering && ShowHeader)
				element = null;
			else if (f?.Target is ObjectPropertyViewModel ovm) {
				element = ovm.ValueModel.Properties[(int)childIndex];
			} else {
				int headerCount = (ShowHeader && !Filtering) ? 1 : 0;
				if (DataContext.ArrangeMode == PropertyArrangeMode.Name)
					element = DataContext.ArrangedEditors[0].Editors[(int)childIndex - headerCount];
				else {
					if (item == null)
						element = DataContext.ArrangedEditors[(int)childIndex - headerCount];
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

			if (DataContext.ArrangeMode == PropertyArrangeMode.Name)
				return false;

			return f.Target is PanelGroupViewModel;
		}
	}
}
