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
				view = (NSTextView)tableView.MakeView (cellIdentifier + "props", this);
				if (view == null) {
					view = new NSTextView () {
						TextContainerInset = new CoreGraphics.CGSize (0, 7),
						Identifier = cellIdentifier + "props",
					};
				}
				((NSTextView)view).Value = property.Property.Name;

				break;
			case "Editors":
				// figure out what type of view model we have
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

				break;
			}

			return view;
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
	}
}
