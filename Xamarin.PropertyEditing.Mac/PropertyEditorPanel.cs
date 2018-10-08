using System;
using System.Collections.Generic;

using CoreGraphics;
using Foundation;
using AppKit;
using Xamarin.PropertyEditing.ViewModels;
using Xamarin.PropertyEditing.Mac.Resources;
using System.ComponentModel;

namespace Xamarin.PropertyEditing.Mac
{
	public partial class PropertyEditorPanel : NSView
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

		public TargetPlatform TargetPlatform
		{
			get { return this.targetPlatform; }
			set
			{
				if (this.viewModel != null) {
					this.viewModel.ArrangedPropertiesChanged -= OnPropertiesChanged;
					this.viewModel.PropertyChanged -= OnVmPropertyChanged;
				}

				this.targetPlatform = value;
				this.viewModel = new PanelViewModel (value);
				this.dataSource = new PropertyTableDataSource (this.viewModel);
				this.propertyTable.Delegate = new PropertyTableDelegate (this.dataSource);
				this.propertyTable.DataSource = this.dataSource;

				if (this.viewModel != null) {
					this.viewModel.ArrangedPropertiesChanged += OnPropertiesChanged;
					this.viewModel.PropertyChanged += OnVmPropertyChanged;
				}
			}
		}

		public ICollection<object> SelectedItems => this.viewModel.SelectedObjects;

		public static Themes.MacThemeManager ThemeManager = new Themes.MacThemeManager ();

		public void Select (IEnumerable<object> selectedItems)
		{
			if (selectedItems == null)
				throw new ArgumentNullException (nameof (selectedItems));

			((ObservableCollectionEx<object>)SelectedItems).Reset (selectedItems);
		}

		private bool isArrangeEnabled = true;
		// when this property changes, need to create new datasource
		private TargetPlatform targetPlatform;
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

			this.propertyArrangeModeLabel = new NSTextField {
				ControlSize = controlSize,
				BackgroundColor = NSColor.Clear,
				Bezeled = false,
				Editable = false,
				StringValue = LocalizationResources.ArrangeByLabel,
				TextColor = NSColor.Black,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			this.propertyArrangeMode = new NSComboBox {
				ControlSize = controlSize,
				Editable = false,
				Font = NSFont.FromFontName (PropertyEditorControl.DefaultFontName, PropertyEditorControl.DefaultFontSize),
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			var enumValues = Enum.GetValues (typeof (PropertyArrangeMode));

			foreach (var item in enumValues) {
				this.propertyArrangeMode.Add (new NSString (item.ToString ())); // TODO May need translating
			}
			this.propertyArrangeMode.SelectItem (0);

			if (IsArrangeEnabled) {
				AddSubview (this.propertyArrangeMode);
				AddSubview (this.propertyArrangeModeLabel);
			}

			this.propertyFilter = new NSSearchField {
				ControlSize = controlSize,
				Font = NSFont.FromFontName (PropertyEditorControl.DefaultFontName, PropertyEditorControl.DefaultFontSize),
				PlaceholderString = LocalizationResources.PropertyFilterLabel,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};
			AddSubview (this.propertyFilter);

			// If either the Filter Mode or PropertySearchFilter Change Filter the Data
			this.propertyArrangeMode.SelectionChanged += OnArrangeModeChanged;
			this.propertyFilter.Changed += OnPropertyFilterChanged;

			this.propertyTable = new FirstResponderOutlineView {
				RefusesFirstResponder = true,
				AutoresizingMask = NSViewResizingMask.WidthSizable,
				SelectionHighlightStyle = NSTableViewSelectionHighlightStyle.None,
				HeaderView = null,
			};

#if DESIGNER_DEBUG
			propertyTable.GridStyleMask = NSTableViewGridStyle.SolidHorizontalLine | NSTableViewGridStyle.SolidVerticalLine;
#endif

			var propertiesList = new NSTableColumn (PropertyListColId) { Title = LocalizationResources.PropertyColumnTitle };
			var propertyEditors = new NSTableColumn (PropertyEditorColId) { Title = LocalizationResources.ValueColumnTitle };
			propertiesList.Width = 158;
			propertyEditors.Width = 250;
			this.propertyTable.AddColumn (propertiesList);
			this.propertyTable.AddColumn (propertyEditors);

			// Set OutlineTableColumn or the arrows showing children/expansion will not be drawn
			this.propertyTable.OutlineTableColumn = propertiesList;

			// create a table view and a scroll view
			var tableContainer = new NSScrollView {
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			// add the panel to the window
			tableContainer.DocumentView = this.propertyTable;
			AddSubview (tableContainer);

			this.DoConstraints (new NSLayoutConstraint[] {
				this.propertyArrangeModeLabel.ConstraintTo(this, (pl, c) => pl.Top == c.Top + 5),
				this.propertyArrangeModeLabel.ConstraintTo(this, (pl, c) => pl.Left == c.Left + 10),

				this.propertyArrangeMode.ConstraintTo(this, (pa, c) => pa.Top == c.Top + 4),
				this.propertyArrangeMode.ConstraintTo(this, (pa, c) => pa.Left == c.Left + 90),
				this.propertyArrangeMode.ConstraintTo(this, (pa, c) => pa.Width == 130),

				this.propertyFilter.ConstraintTo(this, (pf, c) => pf.Top == c.Top + 3),
				this.propertyFilter.ConstraintTo(this, (pf, c) => pf.Left == c.Left + 255),
				this.propertyFilter.ConstraintTo(this, (pa, c) => pa.Width == c.Width - 265),

				tableContainer.ConstraintTo(this, (t, c) => t.Top == c.Top + 30),
				tableContainer.ConstraintTo(this, (t, c) => t.Left == c.Left + 10),
				tableContainer.ConstraintTo(this, (t, c) => t.Width == c.Width - 20),
				tableContainer.ConstraintTo(this, (t, c) => t.Height == c.Height - 37),
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

		private void OnArrangeModeChanged (object sender, EventArgs e)
		{
			Enum.TryParse<PropertyArrangeMode> (this.propertyArrangeMode.GetItemObject (this.propertyArrangeMode.SelectedIndex).ToString (), out PropertyArrangeMode filterMode);
			this.viewModel.ArrangeMode = filterMode;
		}

		private void OnPropertyFilterChanged (object sender, EventArgs e)
		{
			this.viewModel.FilterText = this.propertyFilter.Cell.Title;

			((PropertyTableDelegate)this.propertyTable.Delegate).UpdateExpansions (this.propertyTable);
		}

		void UpdateTheme ()
		{
			Appearance = ThemeManager.CurrentAppearance;
		}

		private void OnVmPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof (PanelViewModel.ArrangeMode))
				OnArrangeModeChanged (sender, e);
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