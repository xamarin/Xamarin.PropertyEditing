using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;

using CoreGraphics;
using Foundation;
using AppKit;
using ObjCRuntime;

using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	public partial class PropertyEditorPanel : NSView
	{
		public PropertyEditorPanel ()
		{
			this.hostResources = new HostResourceProvider ();
			Initialize ();
		}

		public PropertyEditorPanel (IHostResourceProvider hostResources)
		{
			this.hostResources = hostResources;
			Initialize ();
		}

		// Called when created from unmanaged code
		public PropertyEditorPanel (NativeHandle handle) : base (handle)
		{
			this.hostResources = new HostResourceProvider ();
			Initialize ();
		}

		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public PropertyEditorPanel (NSCoder coder) : base (coder)
		{
			this.hostResources = new HostResourceProvider ();
			Initialize ();
		}

		public bool ShowHeader
		{
			get => this.propertyList.ShowHeader;
			set => this.propertyList.ShowHeader = value;
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
					this.viewModel.PropertyChanged -= OnVmPropertyChanged;

					var views = this.tabStack.Views;
					for (int i = 0; i < views.Length; i++) {
						var button = (TabButton)views[i];
						button.Clicked -= OnArrangeModeChanged;
						button.RemoveFromSuperview ();
					}
				}

				this.targetPlatform = value;
				this.viewModel = (value != null) ? new PanelViewModel (value) : null;
				this.propertyList.ViewModel = this.viewModel;

				OnVmPropertyChanged (this.viewModel, new PropertyChangedEventArgs (null));
				if (this.viewModel != null) {
					this.viewModel.PropertyChanged += OnVmPropertyChanged;

					for (int i = 0; i < this.viewModel.ArrangeModes.Count; i++) {
						var item = this.viewModel.ArrangeModes[i];
						string imageName = GetIconName (item.ArrangeMode);
						TabButton arrangeMode = new TabButton (this.hostResources, imageName) {
							Bounds = new CGRect (0, 0, 32, 30),
							Selected = item.IsChecked,
							Tag = i,
							ToolTip = GetTooltip (item.ArrangeMode),
						};

						arrangeMode.Clicked += OnArrangeModeChanged;

						arrangeMode.AccessibilityEnabled = true;
						arrangeMode.AccessibilityTitle = string.Format (Properties.Resources.ArrangeByButtonName, item.ArrangeMode.ToString());

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

		private IHostResourceProvider hostResources;
		private bool isArrangeEnabled = true;
		private TargetPlatform targetPlatform;
		private PropertyList propertyList;
		private PanelViewModel viewModel;

		private NSSearchField propertyFilter;
		private NSStackView tabStack;
		private DynamicBox header, border;

		private void Initialize ()
		{
			this.header = new DynamicBox (HostResourceProvider, NamedResources.PanelTabBackground) {
				ContentViewMargins = new CGSize (0, 0),
				ContentView = new NSView ()
			};
			AddSubview (this.header);

			this.border = new DynamicBox (HostResourceProvider, NamedResources.TabBorderColor) {
				Frame = new CGRect (0, 0, 1, 1)
			};
			header.AddSubview (this.border);

			this.propertyFilter = new NSSearchField {
				PlaceholderString = Properties.Resources.PropertyFilterLabel,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};
			((NSView)this.header.ContentView).AddSubview (this.propertyFilter);

			this.propertyFilter.Changed += OnPropertyFilterChanged;
			this.propertyFilter.AccessibilityEnabled = true;
			this.propertyFilter.AccessibilityTitle = Properties.Resources.AccessibilityPropertyFilter;

			this.tabStack = new NSStackView {
				Orientation = NSUserInterfaceLayoutOrientation.Horizontal,
				TranslatesAutoresizingMaskIntoConstraints = false,
				EdgeInsets = new NSEdgeInsets (0, 0, 0, 0)
			};

			((NSView)this.header.ContentView).AddSubview (this.tabStack);

			this.propertyList = new PropertyList {
				HostResourceProvider = this.hostResources,
				TranslatesAutoresizingMaskIntoConstraints = false
			};
			AddSubview (this.propertyList);

			this.AddConstraints (new[] {
				NSLayoutConstraint.Create (this.header, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this, NSLayoutAttribute.Top, 1, 0),
				NSLayoutConstraint.Create (this.header, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this, NSLayoutAttribute.Width, 1, 0),
				NSLayoutConstraint.Create (this.header, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1, 32),

				NSLayoutConstraint.Create (this.tabStack, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, this.header,  NSLayoutAttribute.Leading, 1, 0),
				NSLayoutConstraint.Create (this.tabStack, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.header, NSLayoutAttribute.Top, 1, 0),
				NSLayoutConstraint.Create (this.tabStack, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, this.header, NSLayoutAttribute.Bottom, 1, 0),
				NSLayoutConstraint.Create (this.tabStack, NSLayoutAttribute.Trailing, NSLayoutRelation.LessThanOrEqual, this.propertyFilter, NSLayoutAttribute.Leading, 1, 0),

				NSLayoutConstraint.Create (this.propertyFilter, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, this.tabStack, NSLayoutAttribute.Trailing, 1, 10),
				NSLayoutConstraint.Create (this.propertyFilter, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, this.header, NSLayoutAttribute.Trailing, 1, -6),
				NSLayoutConstraint.Create (this.propertyFilter, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, this.header, NSLayoutAttribute.CenterY, 1, 0),

				NSLayoutConstraint.Create (this.border, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, this.header, NSLayoutAttribute.Bottom, 1, 0),
				NSLayoutConstraint.Create (this.border, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this.header, NSLayoutAttribute.Width, 1, 0),

				NSLayoutConstraint.Create (this.propertyList, NSLayoutAttribute.Top, NSLayoutRelation.Equal, border, NSLayoutAttribute.Bottom, 1, 0),
				NSLayoutConstraint.Create (this.propertyList, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, this, NSLayoutAttribute.Bottom, 1, 0),
				NSLayoutConstraint.Create (this.propertyList, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this, NSLayoutAttribute.Width, 1, 0),
			});

			UpdateResourceProvider ();
		}

		public sealed override void ViewDidChangeEffectiveAppearance ()
		{
			base.ViewDidChangeEffectiveAppearance ();

			UpdateResourceProvider ();
		}

		private void UpdateResourceProvider ()
		{
			if (this.propertyList != null) {
				this.propertyList.HostResourceProvider = HostResourceProvider;
				this.header.HostResourceProvider = HostResourceProvider;
				this.border.HostResourceProvider = HostResourceProvider;
			}
		}

		private void OnArrangeModeChanged (object sender, EventArgs e)
		{
			this.viewModel.ArrangeMode = this.viewModel.ArrangeModes[(int)((NSView)sender).Tag].ArrangeMode;
		}

		private void OnPropertyFilterChanged (object sender, EventArgs e)
		{
			this.viewModel.FilterText = this.propertyFilter.Cell.Title;
			this.propertyList.UpdateExpansions ();
		}

		private void OnVmPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof (PanelViewModel.ArrangeMode) || String.IsNullOrEmpty (e.PropertyName)) {
				if (this.viewModel != null) {
					int selected = this.viewModel.ArrangeModes.Select (vm => vm.ArrangeMode).IndexOf (this.viewModel.ArrangeMode);
					var views = this.tabStack.Views;
					for (int i = 0; i < views.Length; i++) {
						((TabButton)views[i]).Selected = (i == selected);
					}
				}
			}
		}

		private string GetIconName (PropertyArrangeMode mode)
		{
			switch (mode) {
			case PropertyArrangeMode.Name:
				return "pe-sort-alphabetically-16";
			case PropertyArrangeMode.Category:
				return "pe-group-by-category-16";
			default:
				throw new ArgumentException();
			}
		}

		private string GetTooltip (PropertyArrangeMode mode)
		{
			switch (mode) {
				case PropertyArrangeMode.Name:
					return string.Format("{0} {1}", Properties.Resources.ArrangeByLabel, Properties.Resources.ArrangeByName);
				case PropertyArrangeMode.Category:
					return string.Format ("{0} {1}", Properties.Resources.ArrangeByLabel, Properties.Resources.ArrangeByCategory);
				default:
					throw new ArgumentException ();
			}
		}
	}
}
