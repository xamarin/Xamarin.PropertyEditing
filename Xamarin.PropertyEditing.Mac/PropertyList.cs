using System;
using System.ComponentModel;
using AppKit;
using CoreGraphics;
using Foundation;

using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class PropertyList
		: NSView
	{
		internal const string PropertyEditorColId = "PropertyEditors";

		public PropertyList ()
		{
			this.propertyTable = new FirstResponderOutlineView {
				AccessibilityEnabled = true,
				AccessibilityTitle = Properties.Resources.AccessibilityPropertyTable,
				IndentationPerLevel = 0,
				SelectionHighlightStyle = NSTableViewSelectionHighlightStyle.None,
				HeaderView = null,
				IntercellSpacing = new CGSize (0, 0)
			};

			// NSTableViewStyle.FullWidth is only supported on macOS 11.0 and later
			if (MacSystemInformation.OsVersion >= MacSystemInformation.BigSur)
				this.propertyTable.Style = NSTableViewStyle.FullWidth;

			var propertyEditors = new NSTableColumn (PropertyEditorColId);
			this.propertyTable.AddColumn (propertyEditors);

			this.scrollView = new NSScrollView {
				AutoresizingMask = NSViewResizingMask.WidthSizable | NSViewResizingMask.HeightSizable,
				HasHorizontalScroller = false,
				HasVerticalScroller = true,
			};

			this.scrollView.DocumentView = this.propertyTable;
			AddSubview (this.scrollView);
		}

		public bool ShowHeader
		{
			get { return this.showHeader; }
			set
			{
				if (this.showHeader == value)
					return;

				this.showHeader = value;
				if (this.dataSource != null) {
					this.dataSource.ShowHeader = value;
					this.propertyTable.ReloadData ();
				}
			}
		}

		public PanelViewModel ViewModel
		{
			get { return this.viewModel; }
			set
			{
				if (this.viewModel != null) {
					this.viewModel.ArrangedPropertiesChanged -= OnPropertiesChanged;

					this.propertyTable.Delegate = null;
					this.propertyTable.DataSource = null;
				}

				this.viewModel = value;

				if (this.viewModel != null) {
					this.viewModel.ArrangedPropertiesChanged += OnPropertiesChanged;

					this.dataSource = new PropertyTableDataSource (this.viewModel) { ShowHeader = ShowHeader };
					this.propertyTable.Delegate = new PropertyTableDelegate (HostResourceProvider, this.dataSource);
					this.propertyTable.DataSource = this.dataSource;
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
				UpdateResourceProvider ();
			}
		}

		public sealed override void ViewDidChangeEffectiveAppearance ()
		{
			base.ViewDidChangeEffectiveAppearance ();

			UpdateResourceProvider ();
		}

		public void UpdateExpansions ()
		{
			((PropertyTableDelegate)this.propertyTable.Delegate).UpdateExpansions (this.propertyTable);
		}

		private readonly NSOutlineView propertyTable;
		private readonly NSScrollView scrollView;
		private IHostResourceProvider hostResources;
		private PropertyTableDataSource dataSource;
		private PanelViewModel viewModel;
		private bool showHeader = true;

		private class FirstResponderOutlineView : NSOutlineView
		{
			private bool tabbedIn;
			public override bool ValidateProposedFirstResponder (NSResponder responder, NSEvent forEvent)
			{
				return true;
			}

			public override CGRect FrameOfOutlineCellAtRow (nint row) => CGRect.Empty;

			public override bool BecomeFirstResponder ()
			{
				var willBecomeFirstResponder = base.BecomeFirstResponder ();
				if (willBecomeFirstResponder) {
					if (SelectedRows.Count == 0 && RowCount > 0) {
						SelectRow (0, false);
						this.tabbedIn = true;
						var row = GetRowView ((nint)SelectedRows.FirstIndex, false);
						if (row != null) {
							return Window.MakeFirstResponder (row.NextValidKeyView);
						}
					}
				}
				this.tabbedIn = false;
				return willBecomeFirstResponder;
			}

			public override bool ResignFirstResponder ()
			{
				var wilResignFirstResponder = base.ResignFirstResponder ();
				if (wilResignFirstResponder) {
					if (SelectedRows.Count > 0 && !this.tabbedIn) {
						DeselectRow ((nint)SelectedRows.FirstIndex);
					}
				}
				return wilResignFirstResponder;
			}
		}

		private void OnPropertiesChanged (object sender, EventArgs e)
		{
			this.propertyTable.ReloadData ();
			UpdateExpansions ();
		}

		private void UpdateResourceProvider ()
		{
			if (this.propertyTable == null || this.hostResources == null)
				return;

			this.propertyTable.BackgroundColor = this.hostResources.GetNamedColor (NamedResources.PadBackgroundColor);

			if (this.propertyTable.Delegate != null)
				this.propertyTable.Delegate = new PropertyTableDelegate (HostResourceProvider, this.dataSource);
		}
	}
}
