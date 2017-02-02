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
				if (propertiesChanged != null)
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

			// create columns for the panel
			NSTableColumn propertiesList = new NSTableColumn ("PropertiesList") { Title = "Properties" };
			NSTableColumn propertyEditors = new NSTableColumn ("PropertyEditors") { Title = "Editors" };
			propertiesList.Width = 150;
			propertyEditors.Width = 250;
			propertyTable.AddColumn (propertiesList);
			propertyTable.AddColumn (propertyEditors);

			// add the panel to the window
			tableContainer.DocumentView = propertyTable;
			AddSubview (tableContainer);
		}
	}
}
