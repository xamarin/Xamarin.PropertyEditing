using System;
using System.Collections.Generic;

using CoreGraphics;
using Foundation;
using AppKit;
using Xamarin.PropertyEditing.ViewModels;
using Xamarin.PropertyEditing.Mac.Resources;

namespace Xamarin.PropertyEditing.Mac
{
	public partial class PropertyEditorPanel : AppKit.NSView
	{
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

		public bool IsArrangeEnabled
		{
			get { return this.isArrangeEnabled; }
			set
			{
				if (this.isArrangeEnabled == value)
					return;

				this.isArrangeEnabled = value;
				if (value) {
					AddSubview (this.propertyArrangeMode);
					AddSubview (this.propertyArrangeModeLabel);
				} else {
					this.propertyArrangeMode.RemoveFromSuperview ();
					this.propertyArrangeModeLabel.RemoveFromSuperview ();
				}
			}
		}

		TargetPlatform targetPlatform = TargetPlatform.Default;
		public TargetPlatform TargetPlatform
		{
			get { return targetPlatform; }
			set { targetPlatform = value; }
		}

		public IEditorProvider EditorProvider
		{
			get { return editorProvider; }
			set
			{
				if (this.viewModel != null)
					this.viewModel.ArrangedPropertiesChanged -= OnPropertiesChanged;

				// Populate the Property Table
				editorProvider = value;
				viewModel = new PanelViewModel (editorProvider, TargetPlatform);
				dataSource = new PropertyTableDataSource (viewModel);
				propertyTable.Delegate = new PropertyTableDelegate (dataSource);
				propertyTable.DataSource = dataSource;

				if (this.viewModel != null)
					this.viewModel.ArrangedPropertiesChanged += OnPropertiesChanged;
			}
		}

		public ICollection<object> SelectedItems => this.viewModel.SelectedObjects;

		public static Themes.MacThemeManager ThemeManager = new Themes.MacThemeManager ();

		private bool isArrangeEnabled = true;
		// when this property changes, need to create new datasource
		private IEditorProvider editorProvider;
		private NSOutlineView propertyTable;
		private PropertyTableDataSource dataSource;
		private PanelViewModel viewModel;

		private NSSearchField propertyFilter;
		private NSComboBox propertyArrangeMode;
		private NSTextField propertyArrangeModeLabel;

		// Shared initialization code
		private void Initialize ()
		{
			AutoresizingMask = NSViewResizingMask.WidthSizable | NSViewResizingMask.HeightSizable;

			NSControlSize controlSize = NSControlSize.Small;

			propertyFilter = new NSSearchField (new CGRect (10, Frame.Height - 25, 170, 24)) {
				TranslatesAutoresizingMaskIntoConstraints = false,
				PlaceholderString = LocalizationResources.PropertyFilterLabel,
				ControlSize = controlSize,
				Font = NSFont.FromFontName (PropertyEditorControl.DefaultFontName, PropertyEditorControl.DefaultFontSize),
			};
			AddSubview (propertyFilter);

			this.propertyArrangeModeLabel = new NSTextField (new CGRect (245, Frame.Height - 28, 150, 24)) {
				TranslatesAutoresizingMaskIntoConstraints = false,
				BackgroundColor = NSColor.Clear,
				TextColor = NSColor.Black,
				Editable = false,
				Bezeled = false,
				StringValue = LocalizationResources.ArrangeByLabel,
				ControlSize = controlSize,
			};

			propertyArrangeMode = new NSComboBox (new CGRect (320, Frame.Height - 25, 153, 24)) {
				TranslatesAutoresizingMaskIntoConstraints = false,
				Editable = false,
				ControlSize = controlSize,
				Font = NSFont.FromFontName (PropertyEditorControl.DefaultFontName, PropertyEditorControl.DefaultFontSize),
			};

			var enumValues = Enum.GetValues (typeof (PropertyArrangeMode));

			foreach (var item in enumValues) {
				propertyArrangeMode.Add (new NSString (item.ToString ())); // TODO May need translating
			}
			propertyArrangeMode.SelectItem (0);

			if (IsArrangeEnabled) {
				AddSubview (this.propertyArrangeMode);
				AddSubview (this.propertyArrangeModeLabel);
			}

			// If either the Filter Mode or PropertySearchFilter Change Filter the Data
			propertyArrangeMode.SelectionChanged += OnArrageModeChanged;
			propertyFilter.Changed += OnPropertyFilterChanged;

			propertyTable = new FirstResponderOutlineView {
				RefusesFirstResponder = true,
				AutoresizingMask = NSViewResizingMask.WidthSizable,
				SelectionHighlightStyle = NSTableViewSelectionHighlightStyle.None,
				HeaderView = null,
			};

#if DESIGNER_DEBUG
			propertyTable.GridStyleMask = NSTableViewGridStyle.SolidHorizontalLine | NSTableViewGridStyle.SolidVerticalLine;
#endif

			NSTableColumn propertiesList = new NSTableColumn (PropertyListColId) { Title = LocalizationResources.PropertyColumnTitle };
			NSTableColumn propertyEditors = new NSTableColumn (PropertyEditorColId) { Title = LocalizationResources.ValueColumnTitle };
			propertiesList.Width = 158;
			propertyEditors.Width = 250;
			propertyTable.AddColumn (propertiesList);
			propertyTable.AddColumn (propertyEditors);

			// Set OutlineTableColumn or the arrows showing children/expansion will not be drawn
			propertyTable.OutlineTableColumn = propertiesList;

			// create a table view and a scroll view
			var tableContainer = new NSScrollView (new CGRect (10, Frame.Height - 210, propertiesList.Width + propertyEditors.Width, Frame.Height - 55)) {
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			// add the panel to the window
			tableContainer.DocumentView = propertyTable;
			AddSubview (tableContainer);

			this.DoConstraints (new NSLayoutConstraint[] { 
				propertyFilter.ConstraintTo(this, (pf, c) => pf.Top == c.Top + 3),
				propertyFilter.ConstraintTo(this, (pf, c) => pf.Left == c.Left + 10),

				propertyArrangeModeLabel.ConstraintTo(this, (pl, c) => pl.Top == c.Top + 5),
				propertyArrangeModeLabel.ConstraintTo(propertyArrangeMode, (pl, pa) => pl.Left == pa.Left - 71),

				propertyArrangeMode.ConstraintTo(this, (pa, c) => pa.Top == c.Top + 4),
				propertyArrangeMode.ConstraintTo(this, (pa, c) => pa.Left == c.Left + 280),
				propertyArrangeMode.ConstraintTo(this, (pa, c) => pa.Width == c.Width - 291),

				tableContainer.ConstraintTo(this, (t, c) => t.Top == c.Top + 30),
				tableContainer.ConstraintTo(this, (t, c) => t.Width == c.Width - 20),
				tableContainer.ConstraintTo(this, (t, c) => t.Height == c.Height - 40),
			});

			ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;

			UpdateTheme ();
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				PropertyEditorPanel.ThemeManager.ThemeChanged -= ThemeManager_ThemeChanged;
			}
		}

		void ThemeManager_ThemeChanged (object sender, EventArgs e)
		{
			UpdateTheme ();
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

		void UpdateTheme ()
		{
			this.Appearance = ThemeManager.CurrentAppearance;
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
