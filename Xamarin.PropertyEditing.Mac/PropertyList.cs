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
				IndentationPerLevel = 0,
				RefusesFirstResponder = true,
				SelectionHighlightStyle = NSTableViewSelectionHighlightStyle.None,
				HeaderView = null,
				IntercellSpacing = new CGSize (0, 0)
			};

			var propertyEditors = new NSTableColumn (PropertyEditorColId);
			this.propertyTable.AddColumn (propertyEditors);

			var tableContainer = new NSScrollView {
				AutoresizingMask = NSViewResizingMask.WidthSizable | NSViewResizingMask.HeightSizable
			};

			tableContainer.DocumentView = this.propertyTable;
			AddSubview (tableContainer);
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
					this.dataSource.ShowHeader = false;
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

		public override void ViewDidChangeEffectiveAppearance ()
		{
			if (this.propertyTable == null)
				return;

			this.propertyTable.BackgroundColor = this.hostResources.GetNamedColor (NamedResources.PadBackgroundColor);
		}

		public void UpdateExpansions()
		{
			((PropertyTableDelegate)this.propertyTable.Delegate).UpdateExpansions (this.propertyTable);
		}

		private readonly NSOutlineView propertyTable;
		private IHostResourceProvider hostResources;
		private PropertyTableDataSource dataSource;
		private PanelViewModel viewModel;
		private bool showHeader = true;

		private class FirstResponderOutlineView : NSOutlineView
		{
			[Export ("validateProposedFirstResponder:forEvent:")]
			public bool validateProposedFirstResponder (NSResponder responder, NSEvent ev)
			{
				return true;
			}
		}

		private void OnPropertiesChanged (object sender, EventArgs e)
		{
			this.propertyTable.ReloadData ();

			((PropertyTableDelegate)this.propertyTable.Delegate).UpdateExpansions (this.propertyTable);
		}

		private void UpdateResourceProvider()
		{
			if (this.propertyTable.Delegate != null)
				this.propertyTable.Delegate = new PropertyTableDelegate (HostResourceProvider, this.dataSource);

			ViewDidChangeEffectiveAppearance ();
		}
	}
}
