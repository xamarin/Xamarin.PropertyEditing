using System;
using AppKit;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	public class PropertyTableDelegate : NSTableViewDelegate
	{
		PropertyTableDataSource DataSource;

		public PropertyTableDelegate (PropertyTableDataSource datasource)
		{
			this.DataSource = datasource;
		}

		// the table is looking for this method, picks it up automagically
		public override NSView GetViewForItem (NSTableView tableView, NSTableColumn tableColumn, nint row)
		{
			PropertyViewModel property = DataSource.ViewModel.Properties [(int)row];
			string cellIdentifier;
			NSView view = new NSView ();

			// Setup view based on the column
			// FIXME: could do this differently
			switch (tableColumn.Title) {
			case "Properties":
				cellIdentifier = "cell";
				view = (NSTextView)tableView.MakeView (cellIdentifier, this);
				if (view == null) {
					view = new NSTextView ();
					view.Identifier = cellIdentifier;
				}
				((NSTextView)view).Value = property.Property.Name;

				break;
			case "Editors":
				cellIdentifier = property.GetType ().Name;
				view = tableView.MakeView (cellIdentifier, this);
				if (view == null) {
					if (property is StringPropertyViewModel) {
						view = new StringEditorControl ();
						view.Identifier = cellIdentifier;
					}
				}
				if (property is StringPropertyViewModel)
					((StringEditorControl)view).ViewModel = (StringPropertyViewModel)property;

				break;
			}

			return view;
		}
	}
}
