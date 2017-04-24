using System;
using System.Collections.Generic;
using System.Linq;
using AppKit;
using Foundation;
using Xamarin.PropertyEditing.Mac.ViewModels;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	public class PropertyTableDataSource : NSOutlineViewDataSource
	{
		internal MacPanelViewModel ViewModel { get; private set; }
		public ICollection<object> SelectedItems => ViewModel.SelectedObjects;

		internal PropertyTableDataSource (MacPanelViewModel viewModel)
		{
			ViewModel = viewModel;
		}

		public override nint GetChildrenCount (NSOutlineView outlineView, NSObject item)
		{
			return ViewModel.GetChildrenCount (item);
		}

		public override NSObject GetChild (NSOutlineView outlineView, nint childIndex, NSObject item)
		{
			return ViewModel.GetChildObject ((int)childIndex, item);
		}

		public override bool ItemExpandable (NSOutlineView outlineView, NSObject item)
		{
			return ViewModel.ItemExpandable (item);
		}
	}
}
