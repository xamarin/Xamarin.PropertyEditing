using System;
using System.Collections.Generic;

using CoreGraphics;
using Foundation;
using AppKit;
using Xamarin.PropertyEditing.ViewModels;


namespace Xamarin.PropertyEditing.Mac
{
	public partial class PropertyEditorPanel : AppKit.NSView
	{
		NSSearchField propertyFilter;
		NSComboBox propertyArrangeMode;

		internal const string PropertyListColId = "PropertiesList";
		internal const string PropertyEditorColId = "PropertyEditors";

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
		PanelViewModel viewModel;

		public IEditorProvider EditorProvider
		{
			get { return editorProvider; }
			set
			{
				if (this.viewModel != null)
					this.viewModel.ArrangedPropertiesChanged -= OnPropertiesChanged;

				// Populate the Property Table
				editorProvider = value;
				viewModel = new PanelViewModel (editorProvider);
				dataSource = new PropertyTableDataSource (viewModel);
				propertyTable.Delegate = new PropertyTableDelegate (dataSource);
				propertyTable.DataSource = dataSource;

				if (this.viewModel != null)
					this.viewModel.ArrangedPropertiesChanged += OnPropertiesChanged;
			}
		}

		public ICollection<object> SelectedItems => this.viewModel.SelectedObjects;

		// Shared initialization code
		private void Initialize ()
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
			propertyArrangeMode.SelectionChanged += OnArrageModeChanged;
			propertyFilter.Changed += OnPropertyFilterChanged;

			// create a table view and a scroll view
			var tableContainer = new NSScrollView (new CGRect (10, Frame.Height - 210, Frame.Width - 20, Frame.Height - 55)) {
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			propertyTable = new FirstResponderOutlineView () {
				RefusesFirstResponder = true,
				AutoresizingMask = NSViewResizingMask.WidthSizable,
				SelectionHighlightStyle = NSTableViewSelectionHighlightStyle.None,
				RowHeight = 24,
			};

			// TODO: localize
			NSTableColumn propertiesList = new NSTableColumn (PropertyListColId) { Title = "Properties" };
			NSTableColumn propertyEditors = new NSTableColumn (PropertyEditorColId) { Title = "Value" };
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

		private void OnPropertiesChanged (object sender, EventArgs e)
		{
			this.propertyTable.ReloadData ();

			((PropertyTableDelegate)this.propertyTable.Delegate).UpdateExpansions (this.propertyTable);
		}

		private void OnArrageModeChanged (object sender, EventArgs e)
		{
			PropertyArrangeMode filterMode;
			Enum.TryParse<PropertyArrangeMode> (propertyArrangeMode.GetItemObject (propertyArrangeMode.SelectedIndex).ToString (), out filterMode);
			viewModel.ArrangeMode = filterMode;
		}

		private void OnPropertyFilterChanged (object sender, EventArgs e)
		{
			viewModel.FilterText = propertyFilter.Cell.Title;

			((PropertyTableDelegate)this.propertyTable.Delegate).UpdateExpansions (this.propertyTable);
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
				var obj = (NSObjectFacade)ItemAtRow (row);
				if (obj.Target is IGroupingList<string, PropertyViewModel>)
					return new CGRect (8, 11, 10, 10);

				return base.FrameOfOutlineCellAtRow (row);
			}
		}
	}
}
