using System;
using AppKit;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class ResourceTableDataSource
		: NSTableViewDataSource
	{
		private ResourceSelectorViewModel viewModel;
		internal ResourceTableDataSource (ResourceSelectorViewModel resourceSelectorViewModel)
		{
			if (resourceSelectorViewModel == null)
				throw new ArgumentNullException (nameof (resourceSelectorViewModel));

			this.viewModel = resourceSelectorViewModel;
		}

		public ResourceSelectorViewModel ViewModel => this.viewModel;
		public nint ResourceCount => this.viewModel.Resources.Count;

		public override nint GetRowCount (NSTableView tableView)
		{
			return ResourceCount;
		}
	}
}
