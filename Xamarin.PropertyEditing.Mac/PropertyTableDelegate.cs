﻿using System;
using System.Collections.Generic;
using AppKit;
using Foundation;
using Xamarin.PropertyEditing.ViewModels;
using Xamarin.PropertyEditing.Mac.ViewModels;

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
			} else {
				cellIdentifier = facade.CategoryName;
			}

			// Setup view based on the column
			switch (tableColumn.Identifier) {
				case PropertyEditorPanel.PropertyListTitle:
					view = outlineView.MakeView (cellIdentifier + "props", this);
					if (view == null) {
						view = new UnfocusableTextView (new CoreGraphics.CGRect (0, -5, 75, 20), property.Property.Name) {
							TextContainerInset = new CoreGraphics.CGSize (0, 9),
							Identifier = cellIdentifier + "props",
							Alignment = NSTextAlignment.Right,
						};
					}
					return view;

				case PropertyEditorPanel.PropertyEditorTitle:
					if (!String.IsNullOrEmpty (facade.CategoryName)) {
						var editor = (PropertyEditorControl)outlineView.MakeView (cellIdentifier + "edits", this);
						if (editor == null) {
							Type controlType;
							Type propertyType = property.GetType ();
							if (viewModelTypes.TryGetValue (propertyType, out controlType)) {
								editor = SetUpEditor (controlType, property, outlineView);
							} else {
								if (propertyType.IsGenericType) {
									Type genericType = propertyType.GetGenericTypeDefinition ();
									if (genericType == typeof (EnumPropertyViewModel<>))
										controlType = typeof (EnumEditorControl<>).MakeGenericType (property.Property.Type);
									editor = SetUpEditor (controlType, property, outlineView);
								}
							}
						}

						// we must reset these every time, as the view may have been reused
						editor.ViewModel = property;
						//editor.TableRow = row;
						return editor;
					}
					break;
			}

			throw new Exception ("Unknown column identifier: " + tableColumn.Identifier);
		}

		public override bool ShouldSelectItem (NSOutlineView outlineView, NSObject item)
		{
			var facade = (item as NSObjectFacade);
			// Don't allow selecttion if CategoryName is populated
			return (string.IsNullOrEmpty (facade.CategoryName));
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
