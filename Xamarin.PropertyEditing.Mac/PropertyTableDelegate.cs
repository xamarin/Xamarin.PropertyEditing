using System;
using System.Collections.Generic;
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

			// FIXME: see how this works for now
			Dictionary<Type, int> viewModelTypes = new Dictionary<Type, int>
			{
				{typeof (StringPropertyViewModel), 0}
			};

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
				var type = property.GetType ();
				cellIdentifier = type.Name;
				view = tableView.MakeView (cellIdentifier, this);

				if (viewModelTypes.ContainsKey (type)) {
					// set up the editor based on the type of view model
					switch (viewModelTypes [type]){
					case 0:
						if (view == null) {
							view = new StringEditorControl ();
							view.Identifier = cellIdentifier;
						}
						((StringEditorControl)view).ViewModel = (StringPropertyViewModel)property;
						break;
					}
				}
				break;
			}

			return view;
		}
	}
}
