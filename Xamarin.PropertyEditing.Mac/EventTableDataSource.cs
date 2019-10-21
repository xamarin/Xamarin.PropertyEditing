using System;
using AppKit;
using Foundation;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class EventTableDataSource : BaseOutlineViewDataSource
	{
		public EventTableDataSource (PanelViewModel panelVm) : base (panelVm)
		{
		}

		public override nint GetChildrenCount (NSOutlineView outlineView, NSObject item)
		{
			return DataContext.Events.Count;
		}

		public override NSObject GetChild (NSOutlineView outlineView, nint childIndex, NSObject item)
		{
			return GetFacade (DataContext.Events.ElementAt ((int)childIndex));
		}

		public override bool ItemExpandable (NSOutlineView outlineView, NSObject item)
		{
			return false;
		}
	}
}