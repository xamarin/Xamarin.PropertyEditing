using System;
using Foundation;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class ResourceBrushPropertyViewDelegate : ResourceOutlineViewDelegate
	{
		public BrushPropertyViewModel ViewModel
		{
			get;
			set;
		}

		public override void SelectionDidChange (NSNotification notification)
		{
			var view = notification.Object as ResourceOutlineView;
			var source = view.DataSource as ResourceDataSource;

			var facade = view.ItemAtRow (view.SelectedRow);
			var resource = (facade as NSObjectFacade)?.Target as Resource;

			ViewModel.Resource = resource;
		}
	}
}
