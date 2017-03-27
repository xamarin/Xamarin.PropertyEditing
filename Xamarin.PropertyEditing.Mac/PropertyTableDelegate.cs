using System;
using System.Collections.Generic;
using AppKit;
using Foundation;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	public class PropertyTableDelegate : NSOutlineViewDelegate
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
		public override NSView GetView (NSOutlineView outlineView, NSTableColumn tableColumn, NSObject item)
		{
			var facade = (item as NSObjectFacade);
			PropertyViewModel property = (PropertyViewModel)facade.WrappedObject; ;

			string cellIdentifier;
			if (string.IsNullOrEmpty (facade.CategoryName)) {
				cellIdentifier = property.Property.Name;
			}
			else {
				cellIdentifier = facade.CategoryName;
			}

			NSView view = new NSView ();

			// Setup view based on the column
			switch (tableColumn.Title) {
				case "Properties":
					view = (NSTextView)outlineView.MakeView (cellIdentifier + "props", this);
					if (view == null) {
						view = new NSTextView () {
							TextContainerInset = new CoreGraphics.CGSize (0, 7),
							Identifier = cellIdentifier + "props",
						};
					}

					((NSTextView)view).Value = cellIdentifier;
					break;
				case "Editors":
					if (string.IsNullOrEmpty (facade.CategoryName)) {
						// figure out what type of view model we have
						view = outlineView.MakeView (cellIdentifier + "edits", this);

						// we don't need to do any setup if the editor already exists
						if (view != null)
							return view;

						Type controlType;
						var propertyType = property.GetType ();
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
					}
					break;
			}

			return view;
		}

		public override bool ShouldSelectItem (NSOutlineView outlineView, NSObject item)
		{
			var facade = (item as NSObjectFacade);
			// Don't allow selecttion if CategoryName is populated
			return (string.IsNullOrEmpty (facade.CategoryName));
		}

		public override void ItemDidCollapse (NSNotification notification)
		{
			var facade = notification.UserInfo.ObjectForKey (new NSString ("NSObject")) as NSObjectFacade;
			if (!string.IsNullOrEmpty (facade.CategoryName)) {
				DataSource.ViewModel.ExpandedNode[facade.CategoryName] = false;
			}
		}

		public override void ItemDidExpand (NSNotification notification)
		{
			var facade = notification.UserInfo.ObjectForKey (new NSString ("NSObject")) as NSObjectFacade;
			if (!string.IsNullOrEmpty (facade.CategoryName)) {
				DataSource.ViewModel.ExpandedNode[facade.CategoryName] = true;
			}
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
