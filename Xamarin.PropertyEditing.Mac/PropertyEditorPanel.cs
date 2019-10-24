using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;

using CoreGraphics;
using Foundation;
using AppKit;

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
		public PropertyEditorPanel (IntPtr handle) : base (handle)
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

					var arrangeModeViews = this.arrangeModeTabStack.Views;
					for (int i = 0; i < arrangeModeViews.Length; i++) {
						((TabButton)arrangeModeViews[i]).Clicked -= OnArrangeModeChanged;
					}

					var propertiesEventsViews = this.propertiesEventsTabStack.Views;
					for (int i = 0; i < propertiesEventsViews.Length; i++) {
						((TabButton)propertiesEventsViews[i]).Clicked -= OnPropertiesEventsChanged;
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
							Tag = i,
							Selected = item.IsChecked
						};

						arrangeMode.Clicked += OnArrangeModeChanged;

						this.arrangeModeTabStack.AddView (arrangeMode, NSStackViewGravity.Top);
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
		private NSStackView arrangeModeTabStack;
		private NSStackView propertiesEventsTabStack;
		private DynamicBox header, border;

		private EventList eventList;
		internal const string PropertyEditorColId = "PropertyEditors";
		internal const string EventEditorColId = "EventEditors";

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

			this.arrangeModeTabStack = new NSStackView {
				Orientation = NSUserInterfaceLayoutOrientation.Horizontal,
				TranslatesAutoresizingMaskIntoConstraints = false,
				EdgeInsets = new NSEdgeInsets (0, 0, 0, 0)
			};

			((NSView)this.header.ContentView).AddSubview (this.arrangeModeTabStack);

			this.propertiesEventsTabStack = new NSStackView {
				Orientation = NSUserInterfaceLayoutOrientation.Horizontal,
				TranslatesAutoresizingMaskIntoConstraints = false,
				EdgeInsets = new NSEdgeInsets (0, 0, 0, 0)
			};

			((NSView)this.header.ContentView).AddSubview (this.propertiesEventsTabStack);

			this.propertyList = new PropertyList (this.hostResources, PropertyEditorColId) {
				TranslatesAutoresizingMaskIntoConstraints = false
			};
			AddSubview (this.propertyList);

			AddConstraints (new[] {
				NSLayoutConstraint.Create (this.header, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this, NSLayoutAttribute.Top, 1, 0),
				NSLayoutConstraint.Create (this.header, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this, NSLayoutAttribute.Width, 1, 0),
				NSLayoutConstraint.Create (this.header, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1, 30),

				NSLayoutConstraint.Create (this.arrangeModeTabStack, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this.header,  NSLayoutAttribute.Left, 1, 0),
				NSLayoutConstraint.Create (this.arrangeModeTabStack, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.header, NSLayoutAttribute.Top, 1, 0),
				NSLayoutConstraint.Create (this.arrangeModeTabStack, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, this.header, NSLayoutAttribute.Bottom, 1, 0),
				NSLayoutConstraint.Create (this.arrangeModeTabStack, NSLayoutAttribute.Right, NSLayoutRelation.LessThanOrEqual, this.propertiesEventsTabStack, NSLayoutAttribute.Left, 1, 0),

				NSLayoutConstraint.Create (this.propertiesEventsTabStack, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.header, NSLayoutAttribute.Top, 1, 0),
				NSLayoutConstraint.Create (this.propertiesEventsTabStack, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, this.header, NSLayoutAttribute.Bottom, 1, 0),
				NSLayoutConstraint.Create (this.propertiesEventsTabStack, NSLayoutAttribute.Left, NSLayoutRelation.GreaterThanOrEqual, this.arrangeModeTabStack, NSLayoutAttribute.Right, 1, 0),
				NSLayoutConstraint.Create (this.propertiesEventsTabStack, NSLayoutAttribute.Right, NSLayoutRelation.LessThanOrEqual, this.propertyFilter, NSLayoutAttribute.Left, 1, 0),

				NSLayoutConstraint.Create (this.propertyFilter, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this.header, NSLayoutAttribute.Right, 1, -15),
				NSLayoutConstraint.Create (this.propertyFilter, NSLayoutAttribute.Width, NSLayoutRelation.Equal, 1, 150),
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
					var views = this.arrangeModeTabStack.Views;
					for (int i = 0; i < views.Length; i++) {
						((TabButton)views[i]).Selected = i == selected;
					}
				}
			}

			if (e.PropertyName == nameof (PanelViewModel.EventsEnabled)) {
				if (this.viewModel.EventsEnabled) {
					var sepButton = new UnfocusableButton (this.hostResources, "pe-header-separator");

					this.propertiesEventsTabStack.AddView (sepButton, NSStackViewGravity.Top);

					var properties = new TabButton (this.hostResources, "pe-show-properties-16") {
						Bounds = new CGRect (0, 0, 32, 30),
						Selected = true,
						Tag = 0,
						ToolTip = Properties.Resources.PropertiesSelectedElement,
					};

					properties.Clicked += OnPropertiesEventsChanged;

					this.propertiesEventsTabStack.AddView (properties, NSStackViewGravity.Top);

					var events = new TabButton (this.hostResources, "pe-show-events-16") {
						Bounds = new CGRect (0, 0, 32, 30),
						Tag = 1,
						ToolTip = Properties.Resources.EventHandlersSelectedElement,
					};

					events.Clicked += OnPropertiesEventsChanged;

					this.propertiesEventsTabStack.AddView (events, NSStackViewGravity.Top);

					this.eventList = new EventList (this.hostResources, EventEditorColId) {
						Hidden = true,
						TranslatesAutoresizingMaskIntoConstraints = false,
						ViewModel = this.viewModel,
					};
					AddSubview (this.eventList);

					AddConstraints (new[]{
						NSLayoutConstraint.Create (this.propertiesEventsTabStack, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this.arrangeModeTabStack,  NSLayoutAttribute.Right, 1, 4),
						NSLayoutConstraint.Create (this.eventList, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.border, NSLayoutAttribute.Bottom, 1, 0),
						NSLayoutConstraint.Create (this.eventList, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, this, NSLayoutAttribute.Bottom, 1, 0),
						NSLayoutConstraint.Create (this.eventList, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this, NSLayoutAttribute.Width, 1, 0),
					});
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

		private void OnPropertiesEventsChanged (object sender, EventArgs e)
		{
			if (sender is TabButton tabButton) {
				// Reset them all
				NSView[] views = this.propertiesEventsTabStack.Views;
				for (int i = 0; i < views.Length; i++) {
					if (views[i] is TabButton tb) {
						tb.Selected = false;
					}
				}

				switch (tabButton.Tag) {
					case 0:
						this.propertyList.Hidden = false;
						this.eventList.Hidden = true;
						break;
					case 1:
						this.propertyList.Hidden = true;
						this.eventList.Hidden = false;
						this.eventList.ReloadDate ();
						break;
				}

				tabButton.Selected = true;
			}
		}
	}
}