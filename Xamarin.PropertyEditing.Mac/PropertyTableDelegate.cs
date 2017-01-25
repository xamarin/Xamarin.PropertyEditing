using System;
using AppKit;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	public class PropertyTableDelegate : NSTableViewDelegate
	{
		const string CellIdentifier = "Cell";
		PropertyTableDataSource DataSource;

		public PropertyTableDelegate (PropertyTableDataSource datasource)
		{
			this.DataSource = datasource;
		}

		// the table is looking for this method, picks it up automagically
		public override NSView GetViewForItem (NSTableView tableView, NSTableColumn tableColumn, nint row)
		{
			var property = DataSource.ViewModel.Properties [(int)row];

			// This pattern allows you reuse existing views when they are no-longer in use.
			// If the returned view is null, you instance up a new view
			// If a non-null view is returned, you modify it enough to reflect the new data

			// TODO: need to check CellIdentifier here?
			NSView view = (NSView)tableView.MakeView (CellIdentifier, this);
			if (view == null) {
				if (property is StringPropertyViewModel) {
					view = new StringEditorControl ();
					view.Identifier = CellIdentifier;
				}
			}

			// TODO: can do this differently
			// Setup view based on the column selected
			switch (tableColumn.Title) {
			case "Properties":
				//view.StringValue = DataSource.Properties [(int)row].Name;
				if (view is StringEditorControl) {
					((StringEditorControl)view).StringEditor.StringValue = property.Property.Name;
				}
				break;
			case "Editors":
				//view.StringValue = DataSource.Properties [(int)row].PropertyValue;
				if (view is StringEditorControl) {
					((StringEditorControl)view).StringEditor.StringValue = property.Property.Type.ToString ();
				}
				break;
			}

			return view;
		}
	}
}
