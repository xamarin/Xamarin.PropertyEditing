using System;
using System.Collections.Generic;
using System.Linq;
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
		INotifyCollectionChanged propertiesChanged;
		public IEditorProvider EditorProvider {
			get { return editorProvider; }
			set {
				// Populate the Properety Table
				editorProvider = value;
				viewModel = new PanelViewModel (editorProvider);
				dataSource = new PropertyTableDataSource (viewModel);
				propertyTable.DataSource = dataSource;
				propertyTable.Delegate = new PropertyTableDelegate (dataSource);
				propertiesChanged = dataSource.ViewModel.Properties as INotifyCollectionChanged;
				propertiesChanged.CollectionChanged += (sender, e) => { propertyTable.ReloadData (); };
			}
		}

		public ICollection<object> SelectedItems => this.dataSource.SelectedItems;

		// Shared initialization code
		void Initialize ()
		{
			AutoresizingMask = NSViewResizingMask.WidthSizable | NSViewResizingMask.HeightSizable;

			// create a table view and a scroll view
			NSScrollView tableContainer = new NSScrollView (Frame) {
				AutoresizingMask = NSViewResizingMask.WidthSizable|NSViewResizingMask.HeightSizable
			};
			propertyTable = new NSTableView () {
				AutoresizingMask = NSViewResizingMask.WidthSizable
			};

			//// create columns for our table
			NSTableColumn propertiesList = new NSTableColumn ("PropertiesList") { Title = "Properties" };
			NSTableColumn propertyEditors = new NSTableColumn ("PropertyEditors") { Title = "Editors" };
			propertiesList.Width = 150;
			propertyEditors.Width = 250;

			propertyTable.AddColumn (propertiesList);
			propertyTable.AddColumn (propertyEditors);

			// Create the Product Table Data Source and populate it
			//var dataSource = EditorProvider; 
			// TODO: for each Property in Properties, generate data to add to table

			tableContainer.DocumentView = propertyTable;
			AddSubview (tableContainer);
		}
	}

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
				break;
			}

			return view;
		}
	}

	public class PropertyTableDataSource : NSTableViewDataSource
	{
		internal PropertyTableDataSource (PanelViewModel viewModel /*IEditorProvider data*/)
		{
			ViewModel = viewModel; //new PanelViewModel (data);
		}

		internal PanelViewModel ViewModel { get; private set; }
		public ICollection<object> SelectedItems => this.ViewModel.SelectedObjects;

		public override nint GetRowCount (NSTableView tableView)
		{
			return ViewModel.Properties.Count;
		}
	}
}
