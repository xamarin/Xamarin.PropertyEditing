using System;
using System.Linq;
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

		public PropertyArrangeMode ArrangeMode
		{
			get => this.viewModel.ArrangeMode;
			set => this.viewModel.ArrangeMode = value;
		}

		public bool IsArrangeEnabled
		{
			get { return this.isArrangeEnabled; }
			set
			{
				if (this.isArrangeEnabled == value)
					return;

				this.isArrangeEnabled = value;
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

		public TargetPlatform TargetPlatform
		{
			get { return this.targetPlatform; }
			set
			{
				if (this.viewModel != null) {
					this.viewModel.ArrangedPropertiesChanged -= OnPropertiesChanged;
					this.viewModel.PropertyChanged -= OnVmPropertyChanged;

					var views = this.tabStack.Views;
					for (int i = 0; i < views.Length; i++) {
						((TabButton)views[i]).Clicked -= OnArrangeModeChanged;
					}
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

					for (int i = 0; i < this.viewModel.ArrangeModes.Count; i++) {
						var item = this.viewModel.ArrangeModes[i];
						string imageName = GetIconName (item.ArrangeMode);
						TabButton arrangeMode = new TabButton (this.hostResources, imageName) {
							Bounds = new CGRect (0, 0, 32, 30),
							Tag = i,
							Selected = item.IsChecked
						};

						arrangeMode.Clicked += OnArrangeModeChanged;

						this.tabStack.AddView (arrangeMode, NSStackViewGravity.Top);
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
			if (this.propertyTable == null)
				return;

			this.propertyTable.BackgroundColor = this.hostResources.GetNamedColor (NamedResources.PadBackgroundColor);
		}

		private IHostResourceProvider hostResources = new HostResourceProvider ();
		private bool isArrangeEnabled = true;
		private TargetPlatform targetPlatform;
		private NSOutlineView propertyTable;
		private PropertyTableDataSource dataSource;
		private PanelViewModel viewModel;

		private NSSearchField propertyFilter;
		private NSStackView tabStack;
		private DynamicFillBox header, border;

		private void Initialize ()
		{
			AutoresizingMask = NSViewResizingMask.WidthSizable | NSViewResizingMask.HeightSizable;

			NSControlSize controlSize = NSControlSize.Small;

			this.header = new DynamicFillBox (HostResourceProvider, NamedResources.PanelTabBackground) {
				ContentViewMargins = new CGSize (0, 0),
				ContentView = new NSView ()
			};
			AddSubview (this.header);

			this.border = new DynamicFillBox (HostResourceProvider, NamedResources.TabBorderColor) {
				Frame = new CGRect (0, 0, 1, 1)
			};
			header.AddSubview (this.border);

			this.propertyFilter = new NSSearchField {
				ControlSize = controlSize,
				PlaceholderString = LocalizationResources.PropertyFilterLabel,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};
			((NSView)this.header.ContentView).AddSubview (this.propertyFilter);

			this.propertyFilter.Changed += OnPropertyFilterChanged;

			this.tabStack = new NSStackView {
				Orientation = NSUserInterfaceLayoutOrientation.Horizontal,
				TranslatesAutoresizingMaskIntoConstraints = false,
				EdgeInsets = new NSEdgeInsets (0, 0, 0, 0)
			};

			((NSView)this.header.ContentView).AddSubview (this.tabStack);

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
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			tableContainer.DocumentView = this.propertyTable;
			AddSubview (tableContainer);

			this.AddConstraints (new[] {
				NSLayoutConstraint.Create (header, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this, NSLayoutAttribute.Top, 1, 0),
				NSLayoutConstraint.Create (header, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this, NSLayoutAttribute.Width, 1, 0),
				NSLayoutConstraint.Create (header, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1, 30),

				NSLayoutConstraint.Create (this.tabStack, NSLayoutAttribute.Left, NSLayoutRelation.Equal, header,  NSLayoutAttribute.Left, 1, 0),
				NSLayoutConstraint.Create (this.tabStack, NSLayoutAttribute.Top, NSLayoutRelation.Equal, header, NSLayoutAttribute.Top, 1, 0),
				NSLayoutConstraint.Create (this.tabStack, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, header, NSLayoutAttribute.Bottom, 1, 0),
				NSLayoutConstraint.Create (this.tabStack, NSLayoutAttribute.Right, NSLayoutRelation.LessThanOrEqual, this.propertyFilter, NSLayoutAttribute.Left, 1, 0),

				NSLayoutConstraint.Create (this.propertyFilter, NSLayoutAttribute.Right, NSLayoutRelation.Equal, header, NSLayoutAttribute.Right, 1, -15),
				NSLayoutConstraint.Create (this.propertyFilter, NSLayoutAttribute.Width, NSLayoutRelation.Equal, 1, 150),
				NSLayoutConstraint.Create (this.propertyFilter, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, header, NSLayoutAttribute.CenterY, 1, 0),

				NSLayoutConstraint.Create (border, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, header, NSLayoutAttribute.Bottom, 1, 0),
				NSLayoutConstraint.Create (border, NSLayoutAttribute.Width, NSLayoutRelation.Equal, header, NSLayoutAttribute.Width, 1, 0),

				NSLayoutConstraint.Create (tableContainer, NSLayoutAttribute.Top, NSLayoutRelation.Equal, border, NSLayoutAttribute.Bottom, 1, 0),
				NSLayoutConstraint.Create (tableContainer, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, this, NSLayoutAttribute.Bottom, 1, 0),
				NSLayoutConstraint.Create (tableContainer, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this, NSLayoutAttribute.Width, 1, 0),
			});

			ViewDidChangeEffectiveAppearance ();
		}

		private void UpdateResourceProvider()
		{
			if (this.propertyTable.Delegate != null)
				this.propertyTable.Delegate = new PropertyTableDelegate (HostResourceProvider, this.dataSource);

			this.header.HostResourceProvider = HostResourceProvider;
			this.border.HostResourceProvider = HostResourceProvider;

			ViewDidChangeEffectiveAppearance ();
		}

		private void OnPropertiesChanged (object sender, EventArgs e)
		{
			this.propertyTable.ReloadData ();

			((PropertyTableDelegate)this.propertyTable.Delegate).UpdateExpansions (this.propertyTable);
		}

		private void OnArrangeModeChanged (object sender, EventArgs e)
		{
			this.viewModel.ArrangeMode = this.viewModel.ArrangeModes[(int)((NSView)sender).Tag].ArrangeMode;
		}

		private void OnPropertyFilterChanged (object sender, EventArgs e)
		{
			this.viewModel.FilterText = this.propertyFilter.Cell.Title;

			((PropertyTableDelegate)this.propertyTable.Delegate).UpdateExpansions (this.propertyTable);
		}

		private void OnVmPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof (PanelViewModel.ArrangeMode) || String.IsNullOrEmpty (e.PropertyName)) {
				int selected = this.viewModel.ArrangeModes.Select (vm => vm.ArrangeMode).IndexOf (this.viewModel.ArrangeMode);
				var views = this.tabStack.Views;
				for (int i = 0; i < views.Length; i++) {
					((TabButton)views[i]).Selected = (i == selected);
				}
			}
		}

		private string GetIconName (PropertyArrangeMode mode)
		{
			switch (mode) {
			case PropertyArrangeMode.Name:
				return "sort-alphabetically-16";
			case PropertyArrangeMode.Category:
				return "group-by-category-16";
			default:
				throw new ArgumentException();
			}
		}

		private class FirstResponderOutlineView : NSOutlineView
		{
			[Export ("validateProposedFirstResponder:forEvent:")]
			public bool validateProposedFirstResponder (NSResponder responder, NSEvent ev)
			{
				return true;
			}
		}
	}
}