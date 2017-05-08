using System;
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
using Xamarin.PropertyEditing.Mac.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	public partial class PropertyEditorPanel : AppKit.NSView
	{
		NSSearchField propertyFilter;
		NSComboBox propertyArrangeMode;

		public const string PropertyListTitle = "Property"; // TODO Localise
		public const string PropertyEditorTitle = "Value";  // TODO Localise

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
		NSOutlineView propertyTable;
		PropertyTableDataSource dataSource;
		MacPanelViewModel viewModel;
		INotifyCollectionChanged PropertiesChanged => viewModel?.Properties as INotifyCollectionChanged;

		public IEditorProvider EditorProvider {
			get { return editorProvider; }
			set {
				// if the propertiesChanged is already subscribed to, remove the event
				if (PropertiesChanged != null)
					PropertiesChanged.CollectionChanged -= HandleCollectionChanged;

				// Populate the Properety Table
				editorProvider = value;
				viewModel = new MacPanelViewModel (editorProvider);
				dataSource = new PropertyTableDataSource (viewModel);
				propertyTable.Delegate = new PropertyTableDelegate (dataSource);
				propertyTable.DataSource = dataSource;

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

			propertyFilter = new NSSearchField (new CGRect (10, Frame.Height - 25, 170, 24)) {
				TranslatesAutoresizingMaskIntoConstraints = false,
				PlaceholderString = "Property Filter", // TODO Localize
				ControlSize = NSControlSize.Regular,
			};
			AddSubview (propertyFilter);

			var propertyArrangeModeLabel = new NSTextField (new CGRect (245, Frame.Height - 28, 150, 24)) {
				TranslatesAutoresizingMaskIntoConstraints = false,
				BackgroundColor = NSColor.Clear,
				TextColor = NSColor.Black,
				Editable = false,
				Bezeled = false,
				StringValue = "Arrange By:"
			};
			AddSubview (propertyArrangeModeLabel);

			propertyArrangeMode = new NSComboBox (new CGRect (320, Frame.Height - 25, 153, 24)) {
				TranslatesAutoresizingMaskIntoConstraints = false,
				Editable = false,
				ControlSize = NSControlSize.Regular,
			};
			AddSubview (propertyFilter);


			var enumValues = Enum.GetValues (typeof (PropertyArrangeMode));

			foreach (var item in enumValues) {
				propertyArrangeMode.Add (new NSString (item.ToString ())); // TODO May need translating
			}
			propertyArrangeMode.SelectItem (0);

			AddSubview (propertyArrangeMode);

			// If either the Filter Mode or PropertySearchFilter Change Filter the Data
			propertyArrangeMode.SelectionChanged += PropertyFilterArrangeMode_Changed;
			propertyFilter.Changed += PropertyFilterText_Changed;

			// create a table view and a scroll view
			var tableContainer = new NSScrollView (new CGRect (10, Frame.Height - 210, Frame.Width - 20, Frame.Height - 55)) {
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			propertyTable = new FirstResponderOutlineView () {
				AutoresizingMask = NSViewResizingMask.WidthSizable,
				RowHeight = 24,
			};

			// create columns for the panel
			NSTableColumn propertiesList = new NSTableColumn ("PropertiesList") { Title = PropertyListTitle };
			NSTableColumn propertyEditors = new NSTableColumn ("PropertyEditors") { Title = PropertyEditorTitle };
			propertiesList.Width = 150;
			propertyEditors.Width = 250;
			propertyTable.AddColumn (propertiesList);
			propertyTable.AddColumn (propertyEditors);

			// Set OutlineTableColumn or the arrows showing children/expansion will not be drawn
			propertyTable.OutlineTableColumn = propertiesList;

			// add the panel to the window
			tableContainer.DocumentView = propertyTable;
			AddSubview (tableContainer);

			this.DoConstraints (new NSLayoutConstraint[] { 
				propertyFilter.ConstraintTo(this, (pf, c) => pf.Top == c.Top + 3),
				propertyFilter.ConstraintTo(this, (pf, c) => pf.Left == c.Left + 12),

				propertyArrangeModeLabel.ConstraintTo(this, (pl, c) => pl.Top == c.Top + 5),
				propertyArrangeModeLabel.ConstraintTo(propertyArrangeMode, (pl, pa) => pl.Left == pa.Left - 73),

				propertyArrangeMode.ConstraintTo(this, (pa, c) => pa.Top == c.Top + 3),
				propertyArrangeMode.ConstraintTo(this, (pa, c) => pa.Left == c.Left + 312),
				propertyArrangeMode.ConstraintTo(this, (pa, c) => pa.Width == 154),

				tableContainer.ConstraintTo(this, (t, c) => t.Top == c.Top + 30),
				tableContainer.ConstraintTo(this, (t, c) => t.Width == c.Width - 20),
				tableContainer.ConstraintTo(this, (t, c) => t.Height == c.Height - 40),
			});
		}

		void PropertyFilterArrangeMode_Changed (object sender, EventArgs e)
		{
			PropertyArrangeMode filterMode;
			Enum.TryParse<PropertyArrangeMode> (propertyArrangeMode.GetItemObject (propertyArrangeMode.SelectedIndex).ToString (), out filterMode);
			viewModel.ArrangeMode = filterMode;
		}

		void PropertyFilterText_Changed (object sender, EventArgs e)
		{
			viewModel.FilterText = propertyFilter.Cell.Title;
		}

		class FirstResponderOutlineView : NSOutlineView
		{
			[Export ("validateProposedFirstResponder:forEvent:")]
			public bool validateProposedFirstResponder (NSResponder responder, NSEvent ev)
			{
				return true;
			}

			public override CGRect FrameOfOutlineCellAtRow (nint row)
			{
				var obj = (this.ItemAtRow (row) as NSObjectFacade);
				if (!string.IsNullOrEmpty(obj.CategoryName)) {
					return new CGRect (8, 11, 10, 10);
				}
				else {
					return base.FrameOfOutlineCellAtRow (row);
				}
			}
		}
	}
}
