using System;
using System.Collections.Generic;
using System.Linq;
using AppKit;
using Foundation;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class BindingResourceSelectorControl : NSView
	{
		internal class ResourceOutlineView : NSOutlineView
		{
			private ILookup<ResourceSource, Resource> viewModel;
			public ILookup<ResourceSource, Resource> ViewModel {
				get => this.viewModel;
				set {
					if (this.viewModel != value) {
						this.viewModel = value;
						var dataSource = new ResourceOutlineViewDataSource (this.viewModel);
						Delegate = new ResourceOutlineViewDelegate (dataSource);
						DataSource = dataSource;
					}

					if (this.viewModel != null) {
						ReloadData ();

						ExpandItem (null, true);
					}
				}
			}

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

			[Export ("validateProposedFirstResponder:forEvent:")]
			public bool ValidateProposedFirstResponder (NSResponder responder, NSEvent forEvent)
			{
				return true;
			}

			public void Initialize ()
			{
				AutoresizingMask = NSViewResizingMask.WidthSizable;
				HeaderView = null;
				TranslatesAutoresizingMaskIntoConstraints = false;
			}
		}

		internal class ResourceOutlineViewDelegate : NSOutlineViewDelegate
		{
			private readonly ResourceOutlineViewDataSource dataSource;

			public ResourceOutlineViewDelegate (ResourceOutlineViewDataSource datasource)
			{
				this.dataSource = datasource;
			}

			public override nfloat GetRowHeight (NSOutlineView outlineView, NSObject item)
			{
				return PropertyEditorControl.DefaultControlHeight;
			}

			public override NSView GetView (NSOutlineView outlineView, NSTableColumn tableColumn, NSObject item)
			{
				var labelContainer = (UnfocusableTextField)outlineView.MakeView ("resource", this);
				if (labelContainer == null) {
					labelContainer = new UnfocusableTextField {
						Identifier = "resource",
					};
				}
				var target = (item as NSObjectFacade).Target;

				switch (target) {
				case IGrouping<ResourceSource, Resource> kvp:
					labelContainer.StringValue = kvp.Key.Name;
					break;
				case Resource resource:
					labelContainer.StringValue = resource.Name;
					break;
				default:
					labelContainer.StringValue = "Resource Not Supported";
					break;
				}

				return labelContainer;
			}

			public override bool ShouldSelectItem (NSOutlineView outlineView, NSObject item)
			{
				var target = (item as NSObjectFacade).Target;
				switch (target) {
					case IGrouping<ResourceSource, Resource> kvp:
						return false;
					case Resource resource:
						return true;

					default:
						return false;
				}
			}
		}

		internal class ResourceOutlineViewDataSource : NSOutlineViewDataSource
		{
			public ILookup<ResourceSource, Resource> ViewModel { get; }

			internal ResourceOutlineViewDataSource (ILookup<ResourceSource, Resource> viewModel)
			{
				if (viewModel == null)
					throw new ArgumentNullException (nameof (viewModel));

				ViewModel = viewModel;
			}

			public override nint GetChildrenCount (NSOutlineView outlineView, NSObject item)
			{
				var childCount = 0;
				if (item == null) {
					childCount = this.ViewModel != null ? this.ViewModel.Count () : 0;
				} else {
					var target = (item as NSObjectFacade).Target;
					switch (target) {
						case IGrouping < ResourceSource, Resource> kvp:
							childCount = kvp.Count ();
							break;
						case Resource resource:
							childCount = 0;
							break;
						default:
							childCount = 0;
							break;
					}
				}

				return childCount;
			}

			public override NSObject GetChild (NSOutlineView outlineView, nint childIndex, NSObject item)
			{
				object element;

				if (item == null) {
					element = this.ViewModel.ElementAt ((int)childIndex);
				} else {
					var target = (item as NSObjectFacade).Target;
					switch (target) {
						case IGrouping<ResourceSource, Resource> kvp:
							element = kvp.ElementAt ((int)childIndex);
							break;
						case Resource resource:
							element = resource;
							break;
						default:
							return null;
					}
				}

				return new NSObjectFacade (element);
			}

			public override bool ItemExpandable (NSOutlineView outlineView, NSObject item)
			{
				var target = (item as NSObjectFacade).Target;
				switch (target) {
					case IGrouping<ResourceSource, Resource> kvp:
						return kvp.Count() > 0;
					case Resource resource:
						return false;
					default:
						return false;
				}
			}
		}

		internal const string ResourceSelectorColId = "ResourceSelectorColumn";

		public ResourceOutlineView resourceOutlineView;

		internal BindingResourceSelectorControl (CreateBindingViewModel viewModel)
		{
			TranslatesAutoresizingMaskIntoConstraints = false;

			this.resourceOutlineView = new ResourceOutlineView {

			};
			this.resourceOutlineView.Activated += (sender, e) => {
				if (sender is ResourceOutlineView rov) {
					if (rov.SelectedRow != -1) {
						if (rov.ItemAtRow (rov.SelectedRow) is NSObjectFacade item) {
							if (item.Target is Resource resource) {
								viewModel.SelectedResource = resource;
							}
						}
					}
				}
			};

			var resourceColumn = new NSTableColumn (ResourceSelectorColId);
			this.resourceOutlineView.AddColumn (resourceColumn);

			// Set OutlineTableColumn or the arrows showing children/expansion will not be drawn
			this.resourceOutlineView.OutlineTableColumn = resourceColumn;

			// create a table view and a scroll view
			var outlineViewContainer = new NSScrollView {
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			// add the panel to the window
			outlineViewContainer.DocumentView = this.resourceOutlineView;
			AddSubview (outlineViewContainer);

			AddConstraints (new[] {
				NSLayoutConstraint.Create (outlineViewContainer, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this, NSLayoutAttribute.Top, 1f, 45f),
				NSLayoutConstraint.Create (outlineViewContainer, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this, NSLayoutAttribute.Left, 1f, 5f),
				NSLayoutConstraint.Create (outlineViewContainer, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this, NSLayoutAttribute.Width, 1f, -10f),
				NSLayoutConstraint.Create (outlineViewContainer, NSLayoutAttribute.Height, NSLayoutRelation.Equal,this, NSLayoutAttribute.Height, 1f, -50f),
			});

			viewModel.PropertyChanged += (sender, e) => {
				if (e.PropertyName == nameof (CreateBindingViewModel.ShowResourceSelector)) {
					Hidden = !viewModel.ShowResourceSelector;

					if (viewModel.ShowResourceSelector && viewModel.SourceResources != null) {
							this.resourceOutlineView.ViewModel = viewModel.SourceResources.Value;
					};
				}
			};
		}
	}
}
