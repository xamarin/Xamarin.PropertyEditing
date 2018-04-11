using System;
using System.Collections.Generic;
using System.ComponentModel;
using AppKit;
using CoreGraphics;
using Foundation;
using Xamarin.PropertyEditing.Drawing;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	class ResourceDataSource : NSOutlineViewDataSource
	{
		public ResourceDataSource (ResourceSelectorViewModel viewModel) : base ()
		{
			this.vm = viewModel;
		}

		public override nint GetChildrenCount (NSOutlineView outlineView, NSObject item)
		{
			if (this.vm?.Resources == null)
				return 0;	
			
			if (this.vm.Resources?.Count () == 0)
				return 0;
			
			return this.vm.Resources.Count ();
		}

		public override NSObject GetChild (NSOutlineView outlineView, nint childIndex, NSObject item)
		{
			object element;

			element = (this.vm.Resources.ElementAt((int)childIndex));

			return GetFacade (element);
		}

		public override bool ItemExpandable (NSOutlineView outlineView, NSObject item)
		{
			return false;
		}

		public NSObject GetFacade (object element)
		{
			NSObject facade;

			if (!this.groupFacades.TryGetValue (element, out facade)) {
				this.groupFacades[element] = facade = new NSObjectFacade (element);
			}
			return facade;
		}

		public bool TryGetFacade (object element, out NSObject facade)
		{
			return this.groupFacades.TryGetValue (element, out facade);
		}

		private readonly ResourceSelectorViewModel vm;
		private readonly Dictionary<object, NSObject> groupFacades = new Dictionary<object, NSObject> ();
	}

	class ResourceBrushPropertyViewDelegate : ResourceOutlineViewDelegate {
		public BrushPropertyViewModel ViewModel {
			get;
			set;
		}

		public override void SelectionDidChange (NSNotification notification)
		{
			var view = notification.Object as ResourceOutlineView;
			var source = view.DataSource as ResourceDataSource;

			var facade = view.ItemAtRow (view.SelectedRow);
			var resource = (facade as NSObjectFacade)?.Target as Resource;

			ViewModel.Resource = resource;
		}
	}

	class ResourceOutlineViewDelegate : NSOutlineViewDelegate {
		public override NSView GetView (NSOutlineView outlineView, NSTableColumn tableColumn, NSObject item)
		{
			var facade = item as NSObjectFacade;
			var resource = facade?.Target as Resource;
			switch (tableColumn.Identifier) {
				case "Preview":
					switch (resource) {
						case Resource<CommonSolidBrush> solid:
							return new CommonBrushView {
								Brush = solid.Value,
								Frame = new CGRect (0, 0, 30, 10)
							};
						case Resource<CommonGradientBrush> gradient:
							return new CommonBrushView {
								Brush = gradient.Value,
								Frame = new CGRect (0, 0, 30, 10)
							};
					}
					return new NSView ();
				case "Label":
				default:
					return new NSTextField () {
						StringValue = resource.Name,
						Bordered = false,
						Editable = false,
						Selectable = false,
						ControlSize = NSControlSize.Small,
						Font = NSFont.FromFontName (PropertyEditorControl.DefaultFontName, PropertyEditorControl.DefaultPropertyLabelFontSize),
						BackgroundColor = NSColor.Clear
					};
			}
		}
	}

	class ResourceOutlineView : NSOutlineView
	{
		public ResourceOutlineView ()
		{
			Initialize ();
		}

		// Called when created from unmanaged code
		public ResourceOutlineView (IntPtr handle) : base (handle)
		{
			Initialize ();
		}

		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public ResourceOutlineView (NSCoder coder) : base (coder)
		{
			Initialize ();
		}

		public void Initialize ()
		{
			var nameColumn = new NSTableColumn ("Label") {
				Title = "Name"
			};
			AddColumn (nameColumn);
			var previewColumn = new NSTableColumn ("Preview") {
				Title = "Preview"
			};
			AddColumn (previewColumn);
		}

		ResourceSelectorViewModel viewModel;
		public ResourceSelectorViewModel ViewModel
		{
			get => viewModel;
			set
			{
				viewModel = value;
				DataSource = new ResourceDataSource (viewModel);
			}
		}
	}

	class ResourceBrushViewController : PropertyViewController<BrushPropertyViewModel>
	{
		ResourceOutlineView resourceSelector;
		ResourceBrushPropertyViewDelegate viewDelegate;

		public ResourceBrushViewController ()
		{
			viewDelegate = new ResourceBrushPropertyViewDelegate ();
		}

		Resource resource;
		protected override void OnPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName) {
				case nameof (BrushPropertyViewModel.Resource):
					if (resource == ViewModel.Resource)
						return;
					
					resource = ViewModel?.Resource;
					UpdateSelection ();
					break;
				case nameof (BrushPropertyViewModel.Solid):
					if (resourceSelector != null)
						resourceSelector.ViewModel = ViewModel?.ResourceSelector;
					break;
				case nameof (BrushPropertyViewModel.Value):
					break;
			}
		}

		void UpdateSelection ()
		{
			if (resourceSelector == null)
				return;
			
			var source = resourceSelector.DataSource as ResourceDataSource;
			if (source == null || ViewModel == null)
				return;
			
			nint index = -1;
			if (ViewModel.Resource != null && source.TryGetFacade (ViewModel?.Resource, out var facade)) {
				index = resourceSelector.RowForItem (facade);
			}
			if (index < 0)
				resourceSelector.DeselectAll (null);
			else
				resourceSelector.SelectRow (index, false);
		}

		protected override void OnViewModelChanged (BrushPropertyViewModel oldModel)
		{
			if (resourceSelector != null) {
				viewDelegate.ViewModel = ViewModel;
				resourceSelector.ViewModel = ViewModel?.ResourceSelector;
			}
		}

		public override void LoadView ()
		{
			viewDelegate.ViewModel = ViewModel;
			View = resourceSelector = new ResourceOutlineView {
				Delegate = viewDelegate
			};

			if (ViewModel != null) {
				resourceSelector.ViewModel = ViewModel?.ResourceSelector;
			}
		}
    }
}
