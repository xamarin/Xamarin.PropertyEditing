using System;
using System.Collections.Generic;
using System.ComponentModel;

using CoreGraphics;
using Foundation;
using AppKit;

using Xamarin.PropertyEditing.ViewModels;
using Xamarin.PropertyEditing.Mac.Resources;

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

		public IHostResourceProvider HostResourceProvider
		{
			get => this.hostResources;
			set
			{
				if (this.hostResources == value)
					return;
				if (value == null)
					throw new ArgumentNullException (nameof (value), "Cannot set HostResourceProvider to null");

				this.hostResources = value;
				if (this.propertyTable.Delegate != null)
					this.propertyTable.Delegate = new PropertyTableDelegate (value, this.dataSource);
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
				this.propertyTable.Delegate = new PropertyTableDelegate (HostResourceProvider, this.dataSource);
				this.propertyTable.DataSource = this.dataSource;

				OnVmPropertyChanged (this.viewModel, new PropertyChangedEventArgs (null));
				if (this.viewModel != null) {
					this.viewModel.ArrangedPropertiesChanged += OnPropertiesChanged;
					this.viewModel.PropertyChanged += OnVmPropertyChanged;

					this.propertyArrangeMode.RemoveAll ();
					foreach (ArrangeModeViewModel item in this.viewModel.ArrangeModes) {
						var itemAsString = new NSString (item.ArrangeMode.ToString ());
						this.propertyArrangeMode.Add (itemAsString);
						if (item.IsChecked)
							this.propertyArrangeMode.Select (itemAsString);
					}
				}
			}
		}

		public ICollection<object> SelectedItems => this.viewModel.SelectedObjects;

		public void Select (IEnumerable<object> selectedItems)
		{
			if (selectedItems == null)
				throw new ArgumentNullException (nameof (selectedItems));

			((ObservableCollectionEx<object>)SelectedItems).Reset (selectedItems);
		}

		public override void ViewDidChangeEffectiveAppearance ()
		{
			this.propertyArrangeMode.Font = HostResourceProvider.GetNamedFont (PropertyEditorControl.DefaultFontName, PropertyEditorControl.DefaultFontSize);
			this.propertyFilter.Font = HostResourceProvider.GetNamedFont (PropertyEditorControl.DefaultFontName, PropertyEditorControl.DefaultFontSize);

			this.propertyTable.BackgroundColor = this.hostResources.GetNamedColor (NamedResources.PadBackgroundColor);
		}

		private IHostResourceProvider hostResources = new HostResourceProvider ();
		private bool isArrangeEnabled = true;
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
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			this.propertyArrangeMode = new NSComboBox {
				ControlSize = controlSize,
				Editable = false,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			if (IsArrangeEnabled) {
				AddSubview (this.propertyArrangeMode);
				AddSubview (this.propertyArrangeModeLabel);
			}

			this.propertyFilter = new NSSearchField {
				ControlSize = controlSize,
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

			var propertyEditors = new NSTableColumn (PropertyEditorColId);
			this.propertyTable.AddColumn (propertyEditors);

			// Set OutlineTableColumn or the arrows showing children/expansion will not be drawn
			this.propertyTable.OutlineTableColumn = propertyEditors;

			// create a table view and a scroll view
			var tableContainer = new NSScrollView {
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			// add the panel to the window
			tableContainer.DocumentView = this.propertyTable;
			AddSubview (tableContainer);

			this.AddConstraints (new[] {
				NSLayoutConstraint.Create (this.propertyArrangeModeLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this, NSLayoutAttribute.Top, 1f, 5f),
				NSLayoutConstraint.Create (this.propertyArrangeModeLabel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this, NSLayoutAttribute.Left, 1f, 10f),

				NSLayoutConstraint.Create (this.propertyArrangeMode, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this, NSLayoutAttribute.Top, 1f, 4f),
				NSLayoutConstraint.Create (this.propertyArrangeMode, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this, NSLayoutAttribute.Left, 1f, 90f),
				NSLayoutConstraint.Create (this.propertyArrangeMode, NSLayoutAttribute.Width, NSLayoutRelation.Equal, 1f, 130f),

				NSLayoutConstraint.Create (this.propertyFilter, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this, NSLayoutAttribute.Top, 1f, 3f),
				NSLayoutConstraint.Create (this.propertyFilter, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this, NSLayoutAttribute.Left, 1f, 255f),
				NSLayoutConstraint.Create (this.propertyFilter, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this, NSLayoutAttribute.Width, 1f, -265f),

				NSLayoutConstraint.Create (tableContainer, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this, NSLayoutAttribute.Top, 1f, 30f),
				NSLayoutConstraint.Create (tableContainer, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this, NSLayoutAttribute.Left, 1f, 10f),
				NSLayoutConstraint.Create (tableContainer, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this, NSLayoutAttribute.Width, 1f, -20f),
				NSLayoutConstraint.Create (tableContainer, NSLayoutAttribute.Height, NSLayoutRelation.Equal, this, NSLayoutAttribute.Height, 1f, -37f),
			});

			ViewDidChangeEffectiveAppearance ();
		}

		private void OnPropertiesChanged (object sender, EventArgs e)
		{
			this.propertyTable.ReloadData ();

			((PropertyTableDelegate)this.propertyTable.Delegate).UpdateExpansions (this.propertyTable);
		}

		private void OnArrangeModeChanged (object sender, EventArgs e)
		{
			this.viewModel.ArrangeMode = this.viewModel.ArrangeModes[(int)this.propertyArrangeMode.SelectedIndex].ArrangeMode;
		}

		private void OnPropertyFilterChanged (object sender, EventArgs e)
		{
			this.viewModel.FilterText = this.propertyFilter.Cell.Title;

			((PropertyTableDelegate)this.propertyTable.Delegate).UpdateExpansions (this.propertyTable);
		}

		private void OnVmPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof (PanelViewModel.ArrangeMode) || String.IsNullOrEmpty (e.PropertyName))
				this.propertyArrangeMode.Select (new NSString (this.viewModel.ArrangeMode.ToString ()));
		}

		private class FirstResponderOutlineView : NSOutlineView
		{
			[Export ("validateProposedFirstResponder:forEvent:")]
			public bool validateProposedFirstResponder (NSResponder responder, NSEvent ev)
			{
				return true;
			}

			public override CGRect GetCellFrame (nint column, nint row)
			{
				var super = base.GetCellFrame (column, row);
				if (column == 0) {
					var obj = (NSObjectFacade)ItemAtRow (row);
					if (obj.Target is PropertyGroupViewModel)
						return new CGRect (0, super.Top, super.Right - (super.Left / 2), super.Height);
				}

				return super;
			}
		}
	}
}