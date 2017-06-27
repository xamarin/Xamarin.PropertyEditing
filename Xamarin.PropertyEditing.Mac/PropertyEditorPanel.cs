﻿using System;
using System.Collections.Generic;
using System.Linq;
using CoreGraphics;
using Foundation;
using AppKit;
using Xamarin.PropertyEditing;
using System.Collections.ObjectModel;
using Xamarin.PropertyEditing.ViewModels;
using System.Diagnostics.Contracts;
using Xamarin.PropertyEditing.Reflection;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;

namespace Xamarin.PropertyEditing.Mac
{
	public partial class PropertyEditorPanel : AppKit.NSView
	{
		NSSearchField propertyFilter;
		NSComboBox propertyFilterMode;

		public PropertyEditorPanel ()
		{
			Initialize ();
		}

		// Called when created from unmanaged code
		public PropertyEditorPanel (IntPtr handle) : base (handle)
		{
			Initialize ();
		}

		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public PropertyEditorPanel (NSCoder coder) : base (coder)
		{
			Initialize ();
		}

		// when this property changes, need to create new datasource
		IEditorProvider editorProvider;
		NSTableView propertyTable;
		PropertyTableDataSource dataSource;
		PanelViewModel viewModel;
		INotifyCollectionChanged PropertiesChanged => viewModel?.Properties as INotifyCollectionChanged;
		public IEditorProvider EditorProvider {
			get { return editorProvider; }
			set {
				// if the propertiesChanged is already subscribed to, remove the event
				if (PropertiesChanged != null)
					PropertiesChanged.CollectionChanged -= HandleCollectionChanged;

				// Populate the Properety Table
				editorProvider = value;
				viewModel = new PanelViewModel (editorProvider);
				dataSource = new PropertyTableDataSource (viewModel);
				propertyTable.DataSource = dataSource;
				propertyTable.Delegate = new PropertyTableDelegate (dataSource);

				// if propertiesChanged exists
				if (PropertiesChanged != null)
					PropertiesChanged.CollectionChanged += HandleCollectionChanged;
			}
		}

		void HandleCollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			propertyTable.ReloadData ();
		}


		public ICollection<object> SelectedItems => this.dataSource.SelectedItems;

		// Shared initialization code
		void Initialize ()
		{
			AutoresizingMask = NSViewResizingMask.WidthSizable | NSViewResizingMask.HeightSizable;

			propertyFilter = new NSSearchField (new CGRect (10, Frame.Height - 25, 170, 24));
			propertyFilter.PlaceholderString = "Property Filter";
			propertyFilter.ControlSize = NSControlSize.Regular;
			AddSubview (propertyFilter);

			var label = new NSTextField (new CGRect (195, Frame.Height - 28, 150, 24)) {
				BackgroundColor = NSColor.Clear,
				TextColor = NSColor.Black,
				Editable = false,
				Bezeled = false,
				ControlSize = NSControlSize.Regular,
				StringValue = "Property Filter Mode"
			};
			AddSubview (label);

			propertyFilterMode = new NSComboBox (new CGRect (320, Frame.Height - 25, 100, 24));
			propertyFilterMode.ControlSize = NSControlSize.Regular;
            AddSubview (propertyFilter);


			var enumValues = Enum.GetValues (typeof (PropertyArrangeMode));

			foreach (var item in enumValues) {
				propertyFilterMode.Add (new NSString (item.ToString ()));
			}
			propertyFilterMode.SelectItem (0);

			AddSubview (propertyFilterMode);

			// If either the Filter Mode or PropertySearchFilter Change Filter the Data
			propertyFilterMode.Changed += PropertyFilterMode_Changed;
			propertyFilter.Changed += PropertyFilterMode_Changed;

			// create a table view and a scroll view
			var tableContainer = new NSScrollView (new CGRect (10, Frame.Height - 240, Frame.Width - 20, Frame.Height - 30)) {
				AutoresizingMask = NSViewResizingMask.WidthSizable | NSViewResizingMask.HeightSizable
			};
			propertyTable = new NSTableView {
				RefusesFirstResponder = true,
				AutoresizingMask = NSViewResizingMask.WidthSizable,
				SelectionHighlightStyle = NSTableViewSelectionHighlightStyle.None,
			};

			// create columns for the panel
			NSTableColumn propertiesList = new NSTableColumn ("PropertiesList") {
				Title = "Properties",
				Width = 200,
			};

			NSTableColumn propertyEditors = new NSTableColumn ("PropertyEditors") {
				Title = "Editors",
				Width = 255,
			};
			propertyTable.AddColumn (propertiesList);
			propertyTable.AddColumn (propertyEditors);

			// add the panel to the window
			tableContainer.DocumentView = propertyTable;
			AddSubview (tableContainer);
		}

		void PropertyFilterMode_Changed (object sender, EventArgs e)
		{
			PropertyArrangeMode filterMode;
			Enum.TryParse<PropertyArrangeMode> (propertyFilterMode.GetItemObject (0).ToString (), out filterMode);
			FilterData (propertyFilter.Cell.Title, filterMode);
		}

		public void FilterData (string title, PropertyArrangeMode filterMode)
		{
			viewModel.FilterData (title, filterMode);
		}

		class FirstResponderTableView : NSTableView
		{
			[Export ("validateProposedFirstResponder:forEvent:")]
			public bool validateProposedFirstResponder (NSResponder responder, NSEvent ev)
			{
				return true;
			}
		}
	}
}
