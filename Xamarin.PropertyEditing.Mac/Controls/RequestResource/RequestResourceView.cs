using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using AppKit;
using CoreGraphics;
using Foundation;
using Xamarin.PropertyEditing.Drawing;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class FirstResponderTableView : NSTableView
	{
		[Export ("validateProposedFirstResponder:forEvent:")]
		public bool validateProposedFirstResponder (NSResponder responder, NSEvent ev)
		{
			return true;
		}
	}

	internal class RequestResourceView
		: BasePopOverViewModelControl
	{
		NSSearchField searchResources;
		NSSegmentedControl segmentedControl;
		NSButton showPreviewImage;
		RequestResourcePanel resourceSelectorPanel;

		public NSPopover PopOver { get; internal set; }

		private bool showPreview;
		public bool ShowPreview
		{
			get { return showPreview; }
			set {
				this.showPreview = value;

				Frame = this.showPreview ? new CGRect (Frame.X, Frame.Y, 640, 380) : new CGRect (Frame.X, Frame.Y, 460, 380);

				if (PopOver != null) {
					PopOver.ContentSize = Frame.Size;
				}
			}
		}

		public RequestResourceView (IHostResourceProvider hostResources, PropertyViewModel propertyViewModel)
			: base (hostResources, propertyViewModel, Properties.Resources.SelectResourceTitle, "pe-resource-editor-32")
		{
			Initialize (propertyViewModel);
		}

		private void Initialize (PropertyViewModel propertyViewModel)
		{
			this.ShowPreview = true;
			TranslatesAutoresizingMaskIntoConstraints = false;

			var FrameWidthThird = Frame.Width / 3;
			var FrameWidthHalf = Frame.Width / 2;
			var FrameHeightHalf = Frame.Height / 2;

			NSControlSize controlSize = NSControlSize.Small;

			this.searchResources = new NSSearchField {
				ControlSize = controlSize,
				Font = NSFont.FromFontName (PropertyEditorControl.DefaultFontName, PropertyEditorControl.DefaultFontSize),
				PlaceholderString = Properties.Resources.SearchResourcesTitle,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			this.searchResources.Changed += OnSearchResourcesChanged;

			AddSubview (this.searchResources);

			var vmType = propertyViewModel.GetType ();
			var valuePropertyInfo = vmType.GetProperty ("Value");
			var resourceValue = valuePropertyInfo.GetValue (propertyViewModel);
			var resourceSelectorPropertyInfo = vmType.GetProperty ("ResourceSelector");
			var resourceSelector = resourceSelectorPropertyInfo.GetValue (propertyViewModel) as ResourceSelectorViewModel;

			if (resourceSelector != null) {
				this.resourceSelectorPanel = new RequestResourcePanel (HostResources, resourceSelector, resourceValue);
			} else {
				this.resourceSelectorPanel = new RequestResourcePanel (HostResources, new ResourceSelectorViewModel (propertyViewModel.TargetPlatform.ResourceProvider, propertyViewModel.Editors.Select (ed => ed.Target), propertyViewModel.Property), resourceValue);
			}
			this.resourceSelectorPanel.ResourceSelected += (sender, e) => {
				propertyViewModel.Resource = this.resourceSelectorPanel.SelectedResource;
			};
			this.resourceSelectorPanel.DoubleClicked += (sender, e) => {
				PopOver.Close ();
			};

			AddSubview (this.resourceSelectorPanel);

			segmentedControl = NSSegmentedControl.FromLabels (new string[] { Properties.Resources.AllResources, Properties.Resources.Local, Properties.Resources.Shared }, NSSegmentSwitchTracking.SelectOne, () => {
				//Switch Resource Types
				switch (this.segmentedControl.SelectedSegment) {
				case 0:
					this.resourceSelectorPanel.ViewModel.ShowBothResourceTypes = true;
					this.segmentedControl.SetImage (HostResources.GetNamedImage ("pe-resource-editor-16"), 2);
					break;
				case 1:
					this.resourceSelectorPanel.ViewModel.ShowOnlyLocalResources = true;
					this.segmentedControl.SetImage (HostResources.GetNamedImage ("pe-resource-editor-16"), 2);
					break;
				case 2:
					this.resourceSelectorPanel.ViewModel.ShowOnlySystemResources = true;
					this.segmentedControl.SetImage (HostResources.GetNamedImage ("pe-resource-editor-16~sel"), 2);
					break;
				}

				this.resourceSelectorPanel.ReloadData ();
			});
			this.segmentedControl.SetImage (HostResources.GetNamedImage ("pe-resource-editor-16"), 2);
			this.segmentedControl.Frame = new CGRect ((FrameWidthThird - (segmentedControl.Bounds.Width) / 2), 5, (Frame.Width - (FrameWidthThird)) - 10, 24);
			this.segmentedControl.Font = NSFont.FromFontName (PropertyEditorControl.DefaultFontName, PropertyEditorControl.DefaultFontSize);
			this.segmentedControl.TranslatesAutoresizingMaskIntoConstraints = false;
			this.segmentedControl.SetSelected (true, 0);
			this.resourceSelectorPanel.ViewModel.ShowBothResourceTypes = true;

			AddSubview (this.segmentedControl);

			this.showPreviewImage = new NSButton {
				Bordered = false,
				ControlSize = controlSize,
				Image = NSImage.ImageNamed (NSImageName.QuickLookTemplate),
				Title = string.Empty,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			this.showPreviewImage.Activated += (o, e) => {
				ShowPreview = !ShowPreview;
				RepositionControls ();
			};

			AddSubview (this.showPreviewImage);

			OnSearchResourcesChanged(null, null);

			RepositionControls ();
		}

		private void RepositionControls ()
		{
			var FrameWidthThird = Frame.Width / 3;
			var FrameWidthHalf = Frame.Width / 2;
			var FrameHeightHalf = Frame.Height / 2;

			this.searchResources.Frame = new CGRect (FrameWidthThird, Frame.Height - 30, (Frame.Width - (FrameWidthThird)) - 10, 30);

			this.showPreviewImage.Frame = new CGRect (Frame.Width - 35, 10, 24, 24);
		}

		private void OnSearchResourcesChanged (object sender, EventArgs e)
		{
			this.resourceSelectorPanel.ViewModel.FilterText = searchResources.Cell.Title;
			this.resourceSelectorPanel.ReloadData ();
		}
	}
}