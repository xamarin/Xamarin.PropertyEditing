using System;
using System.Collections.Generic;
using AppKit;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	public class PropertyTableDelegate : NSTableViewDelegate
	{
		PropertyTableDataSource DataSource;

		Dictionary<Type, Type> viewModelTypes = new Dictionary<Type, Type> {
			{typeof (StringPropertyViewModel), typeof (StringEditorControl)},
			{typeof (IntegerPropertyViewModel), typeof (IntegerNumericEditorControl)},
			{typeof (FloatingPropertyViewModel), typeof (DecimalNumericEditorControl)},
			{typeof (PropertyViewModel<bool>), typeof (BooleanEditorControl)},
			{typeof (PropertyViewModel<CoreGraphics.CGPoint>), typeof (PointEditorControl)},
			{typeof (PropertyViewModel<CoreGraphics.CGRect>), typeof (CGRectEditorControl)},
		};

		public PropertyTableDelegate (PropertyTableDataSource datasource)
		{
			this.DataSource = datasource;
		}

		// the table is looking for this method, picks it up automagically
		public override NSView GetViewForItem (NSTableView tableView, NSTableColumn tableColumn, nint row)
		{
			PropertyViewModel property = DataSource.ViewModel.Properties[(int)row];
			var propertyType = property.GetType ();
			var cellIdentifier = propertyType.Name;

			// Setup view based on the column
			switch (tableColumn.Identifier) {
			case "PropertiesList":
				var view = tableView.MakeView (cellIdentifier + "props", this);
				if (view == null) {
					view = new UnfocusableTextView (new CoreGraphics.CGRect (0, -5, 75, 20), property.Property.Name) {
						TextContainerInset = new CoreGraphics.CGSize (0, 7),
						Identifier = cellIdentifier + "props",
						Alignment = NSTextAlignment.Right,
					};
				}
				return view;

			case "PropertyEditors":
				var editor = (PropertyEditorControl)tableView.MakeView (cellIdentifier  + "edits", this);

				if (editor == null) {
					Type controlType;
					if (viewModelTypes.TryGetValue (propertyType, out controlType)) {
						editor = SetUpEditor (controlType, property, tableView);
					}
					else {
						if (propertyType.IsGenericType) {
							Type genericType = propertyType.GetGenericTypeDefinition ();
							if (genericType == typeof (EnumPropertyViewModel<>))
								controlType = typeof (EnumEditorControl<>).MakeGenericType (property.Property.Type);
							editor = SetUpEditor (controlType, property, tableView);
						}
					}
				}

				// we must reset these every time, as the view may have been reused
				editor.ViewModel = property;
				editor.TableRow = row;
				return editor;
			}

			throw new Exception ("Unknown column identifier: " + tableColumn.Identifier);
		}

		public override nfloat GetRowHeight (NSTableView tableView, nint row)
		{
			/*var col = tableView.TableColumns ()[1];
			var cell = col.DataCellForRow (row);
			var view = tableView.GetView (1, row, false) as PropertyEditorControl;
			if (view != null) {
				return view.Frame.Height;
			}
			else {*/

			return 24;
			//}
		}

		// set up the editor based on the type of view model
		PropertyEditorControl SetUpEditor (Type controlType, PropertyViewModel property, NSTableView table)
		{
			var view = (PropertyEditorControl)Activator.CreateInstance (controlType);
			view.Identifier = property.GetType ().Name;
			view.TableView = table;

			return view;
		}
	}
}
