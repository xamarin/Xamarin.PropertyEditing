using System;
using System.Collections.Generic;
using AppKit;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	public class PropertyTableDataSource : NSTableViewDataSource
	{
		internal PropertyTableDataSource (PanelViewModel viewModel)
		{
			ViewModel = viewModel;
		}

		internal PanelViewModel ViewModel { get; private set; }
		public ICollection<object> SelectedItems => this.ViewModel.SelectedObjects;

		public override nint GetRowCount (NSTableView tableView)
		{
			return ViewModel.Properties.Count;
		}
	}
}
