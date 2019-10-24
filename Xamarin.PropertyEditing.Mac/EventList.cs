using System;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class EventList
		: BaseOutlineList
	{
		public EventList (IHostResourceProvider hostResources, string columnID) : base (hostResources, columnID)
		{
		}

		public override PanelViewModel ViewModel
		{
			set {
				if (this.viewModel != null) {
					OutlineViewTable.Delegate = null;
					OutlineViewTable.DataSource = null;
				}

				this.viewModel = value;

				if (this.viewModel != null) {
					this.dataSource = new EventTableDataSource (this.viewModel);
					OutlineViewTable.Delegate = new EventTableDelegate (HostResourceProvider, this.dataSource);
					OutlineViewTable.DataSource = this.dataSource;
				}
			}
		}

		protected override void UpdateResourceProvider ()
		{
			base.UpdateResourceProvider ();

			if (OutlineViewTable.Delegate == null)
				OutlineViewTable.Delegate = new EventTableDelegate (HostResourceProvider, this.dataSource);
		}

		internal void ReloadDate ()
		{
			OutlineViewTable.ReloadData ();
		}
	}
}