using System;
using System.ComponentModel;
using AppKit;
using CoreGraphics;
using Foundation;

using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class PropertyList
		: BaseOutlineList
	{
		public PropertyList (IHostResourceProvider hostResources, string columnID) : base (hostResources, columnID)
		{
		}

		public bool ShowHeader
		{
			get { return this.showHeader; }
			set
			{
				if (this.showHeader == value)
					return;

				this.showHeader = value;
				if (this.dataSource != null && this.dataSource is PropertyTableDataSource propertyTableDataSource) {
					propertyTableDataSource.ShowHeader = value;
					OutlineViewTable.ReloadData ();
				}
			}
		}

		public override PanelViewModel ViewModel
		{
			set {
				if (this.viewModel != null) {
					this.viewModel.ArrangedPropertiesChanged -= OnPropertiesChanged;

					OutlineViewTable.Delegate = null;
					OutlineViewTable.DataSource = null;
				}

				this.viewModel = value;

				if (this.viewModel != null) {
					this.viewModel.ArrangedPropertiesChanged += OnPropertiesChanged;

					this.dataSource = new PropertyTableDataSource (this.viewModel) { ShowHeader = ShowHeader };
					OutlineViewTable.Delegate = new PropertyTableDelegate (HostResourceProvider, this.dataSource);
					OutlineViewTable.DataSource = this.dataSource;
				}
			}
		}

		public void UpdateExpansions ()
		{
			((PropertyTableDelegate)OutlineViewTable.Delegate).UpdateExpansions (OutlineViewTable);
		}

		private bool showHeader = true;

		private void OnPropertiesChanged (object sender, EventArgs e)
		{
			OutlineViewTable.ReloadData ();

			((PropertyTableDelegate)OutlineViewTable.Delegate).UpdateExpansions (OutlineViewTable);
		}

		protected override void UpdateResourceProvider ()
		{
			base.UpdateResourceProvider ();

			if (OutlineViewTable.Delegate != null)
				OutlineViewTable.Delegate = new PropertyTableDelegate (HostResourceProvider, this.dataSource);
		}
	}
}