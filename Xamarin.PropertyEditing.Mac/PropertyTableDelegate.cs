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
			NSView view = new NSView ();

			// Setup view based on the column
			switch (tableColumn.Title) {
			case "Properties":
				view = (UnfocusableTextField)tableView.MakeView (cellIdentifier + "props", this);
				if (view == null) {
					view = new UnfocusableTextField (new CoreGraphics.CGRect (0, -5, 75, 20), property.Property.Name) {
						TextContainerInset = new CoreGraphics.CGSize (0, 7),
						Identifier = cellIdentifier + "props",
						Alignment = NSTextAlignment.Right,
					};
				}
				//((UnfocusableTextField)view).Value = property.Property.Name;

				break;
			case "Editors":
				view = tableView.MakeView (cellIdentifier  + "edits", this);

				// we don't need to do any setup if the editor already exists
				if (view != null) 
					return view;

				Type controlType;
				if (viewModelTypes.TryGetValue (propertyType, out controlType)) {
					view = SetUpEditor (view, controlType, property);
				}
				else {
					if (propertyType.IsGenericType) {
						Type genericType = propertyType.GetGenericTypeDefinition ();
						if (genericType == typeof (EnumPropertyViewModel<>))
							controlType = typeof (EnumEditorControl<>).MakeGenericType (property.Property.Type);
						view = SetUpEditor (view, controlType, property);
					}
				}
					var lookupRow = row - 1;
					if (lookupRow< 0)
						lookupRow = DataSource.ViewModel.Properties.Count - 1;
				var ViewAtRow = tableView.GetView (1, lookupRow, false);
					if (ViewAtRow != null) {
						ViewAtRow.NextKeyView = view;
						ViewAtRow.NextResponder = view;
					}
						
				break;
			}

			return view;
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
		NSView SetUpEditor (NSView view, Type controlType, PropertyViewModel property)
		{
			if (view == null) {
				view = (PropertyEditorControl)Activator.CreateInstance (controlType);
				view.Identifier = property.GetType ().Name;
			}
			((PropertyEditorControl)view).ViewModel = property;

			return view;
		}

		public override bool ShouldSelectRow (NSTableView tableView, nint row)
		{
			return false;
		}
	}
}
