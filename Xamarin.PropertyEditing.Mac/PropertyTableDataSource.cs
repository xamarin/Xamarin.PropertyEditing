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
			PanelViewModel.ViewModelMap.Add (typeof (CoreGraphics.CGPoint), (p, e) => new PropertyViewModel<CoreGraphics.CGPoint> (p, e));
			PanelViewModel.ViewModelMap.Add (typeof (CoreGraphics.CGRect), (p, e) => new PropertyViewModel<CoreGraphics.CGRect> (p, e));
		}

		internal PanelViewModel ViewModel { get; private set; }
		public ICollection<object> SelectedItems => this.ViewModel.SelectedObjects;

		public override nint GetRowCount (NSTableView tableView)
		{
			return ViewModel.Properties.Count;
		}
	}
}
