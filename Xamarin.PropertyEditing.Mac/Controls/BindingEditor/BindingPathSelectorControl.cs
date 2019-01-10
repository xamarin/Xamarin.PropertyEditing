using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AppKit;
using Foundation;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class PathOutlineView : NSOutlineView
	{
		private IReadOnlyCollection<object> viewModel;
		private PropertyTreeRoot propertyTreeRoot;

		private IReadOnlyCollection<object> ViewModel
		{
			get => this.viewModel;
			set {
				if (this.viewModel != value) {
					this.viewModel = value;

					var dataSource = new PathOutlineViewDataSource (this.viewModel, this.targetName);
					Delegate = new PathOutlineViewDelegate (dataSource);
					DataSource = dataSource;

					ReloadData ();

					ExpandItem (ItemAtRow (0));
				}
			}
		}

		private string targetName { get; set; }
		public PropertyTreeRoot PropertyTreeRoot
		{
			get { return this.propertyTreeRoot; }
			internal set {
				if (this.propertyTreeRoot != value) {
					this.propertyTreeRoot = value;
					if (this.propertyTreeRoot != null) {
						targetName = this.propertyTreeRoot.TargetType.Name;
						ViewModel = this.propertyTreeRoot.Children;
					} else {
						targetName = string.Empty;
						ViewModel = null;
					}
				}
			}
		}

		public PathOutlineView ()
		{
			Initialize ();
		}

		// Called when created from unmanaged code
		public PathOutlineView (IntPtr handle) : base (handle)
		{
			Initialize ();
		}

		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public PathOutlineView (NSCoder coder) : base (coder)
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

	internal class PathOutlineViewDelegate : NSOutlineViewDelegate
	{
		private readonly PathOutlineViewDataSource dataSource;

		public PathOutlineViewDelegate (PathOutlineViewDataSource dataSource)
		{
			this.dataSource = dataSource;
		}

		public override nfloat GetRowHeight (NSOutlineView outlineView, NSObject item)
		{
			return PropertyEditorControl.DefaultControlHeight;
		}

		public override NSView GetView (NSOutlineView outlineView, NSTableColumn tableColumn, NSObject item)
		{
			var labelContainer = (UnfocusableTextField)outlineView.MakeView ("path", this);
			if (labelContainer == null) {
				labelContainer = new UnfocusableTextField {
					Identifier = "path",
				};
			}
			var target = (item as NSObjectFacade).Target;

			switch (target) {
				case PropertyTreeElement propertyTreeElement:
					labelContainer.StringValue = string.Format("{0}: ({1})", propertyTreeElement.Property.Name, propertyTreeElement.Property.RealType.Name);
					break;

				case string targetName:
					labelContainer.StringValue = targetName;
					break;

				default:
					labelContainer.StringValue = "Type Not Supported";
					break;
			}

			return labelContainer;
		}

		public override bool ShouldSelectItem (NSOutlineView outlineView, NSObject item)
		{
			if (item is NSObjectFacade facade) {
				switch (facade.Target) {
					case PropertyTreeElement propertyTreeElement:
						var propertyTreeResult = propertyTreeElement.Children.Task.Result;
						return propertyTreeResult.Count == 0;

					default:
						return false;
				}
			} else {
				return false;
			}
		}
	}

	internal class PathOutlineViewDataSource : NSOutlineViewDataSource
	{
		private readonly IReadOnlyCollection<object> viewModel;
		private string targetName;

		internal PathOutlineViewDataSource (IReadOnlyCollection<object> viewModel, string targetName)
		{
			this.viewModel = viewModel;
			this.targetName = targetName;
		}

		public override nint GetChildrenCount (NSOutlineView outlineView, NSObject item)
		{
			nint childCount;
			if (item == null) {
				childCount = this.viewModel != null ? this.viewModel.Count () + 1 : 0;
			} else {
				var target = (item as NSObjectFacade).Target;
				switch (target) {
					case PropertyTreeElement propertyTreeElement:
						IReadOnlyCollection<PropertyTreeElement> propertyTrees = propertyTreeElement.Children.Task.Result;
						childCount = propertyTrees.Count;
						break;

					case string targetName:
						childCount = this.viewModel.Count ();
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
			if (childIndex == 0 && item == null) {
				return new NSObjectFacade (targetName);
			}

			if (item is NSObjectFacade objectFacade) {
				var target = objectFacade.Target;
				switch (target) {
					case PropertyTreeElement propertyTreeElement:
						IReadOnlyCollection<PropertyTreeElement> propertyTrees = propertyTreeElement.Children.Task.Result;
						return new NSObjectFacade (propertyTrees.ElementAt ((int)childIndex));

					case string targetName:
						return new NSObjectFacade (this.viewModel.ElementAt ((int)childIndex));

					default:
						return null;
				}

			}
			return null;
		}

		public override bool ItemExpandable (NSOutlineView outlineView, NSObject item)
		{
			if (item is NSObjectFacade objectFacade) {
				var target = objectFacade.Target;
				switch (target) {
					case PropertyTreeElement propertyTreeElement:
						IReadOnlyCollection<PropertyTreeElement> propertyTrees = propertyTreeElement.Children.Task.Result;
						return propertyTrees.Count > 0;

					case string targetName:
						return true;

					default:
						return false;
				}
			}

			return false;
		}
	}

	internal class BindingPathSelectorControl : NSView
	{
		private PathOutlineView pathOutlineView;
		internal const string PathSelectorColumnColId = "PathSelectorColumn";
		private const float spacing = 10f;
		private NSTextField customPath;
		public NSTextField CustomPath { get; private set; }

		public BindingPathSelectorControl (CreateBindingViewModel viewModel)
		{
			TranslatesAutoresizingMaskIntoConstraints = false;

			var customCheckBox = new NSButton {
				ControlSize = NSControlSize.Small,
				Font = NSFont.FromFontName (PropertyEditorControl.DefaultFontName, PropertyEditorControl.DefaultFontSize),
				Title = Properties.Resources.Custom,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			customCheckBox.SetButtonType (NSButtonType.Switch);

			AddSubview (customCheckBox);
			AddConstraints (new[] {
				NSLayoutConstraint.Create (customCheckBox, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this, NSLayoutAttribute.Top, 1f, 8f),
				NSLayoutConstraint.Create (customCheckBox, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this, NSLayoutAttribute.Right, 1f, -90f),
			});

			this.customPath = new NSTextField {
				ControlSize = NSControlSize.Mini,
				Enabled = false,
				Font = NSFont.FromFontName (PropertyEditorControl.DefaultFontName, PropertyEditorControl.DefaultFontSize),
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			var customPathHeightConstraint = NSLayoutConstraint.Create (this.customPath, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 0);

			AddSubview (this.customPath);
			AddConstraints (new[] {
				NSLayoutConstraint.Create (this.customPath, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this, NSLayoutAttribute.Top, 1f, 45f),
				NSLayoutConstraint.Create (this.customPath, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this, NSLayoutAttribute.Width, 1f, -10f),
				NSLayoutConstraint.Create (this.customPath, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, this, NSLayoutAttribute.CenterX, 1, 0),
				customPathHeightConstraint,
			});

			// create a table view and a scroll view
			var outlineViewContainer = new NSScrollView {
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			var outlineViewContainerTopConstraint = NSLayoutConstraint.Create (outlineViewContainer, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.customPath, NSLayoutAttribute.Bottom, 1f, 0f);
			customCheckBox.Activated += (sender, e) => {
				this.customPath.Enabled = customCheckBox.State == NSCellStateValue.On;
				customPathHeightConstraint.Constant = this.customPath.Enabled ? PropertyEditorControl.DefaultControlHeight : 0;
				outlineViewContainerTopConstraint.Constant = this.customPath.Enabled ? 10 : 0;
				SetCustomPath (viewModel);
			};

			this.customPath.Changed += (sender, e) => {
				viewModel.Path = this.customPath.StringValue;
			};

			this.pathOutlineView = new PathOutlineView {

			};

			this.pathOutlineView.Activated += (sender, e) => {
				if (sender is PathOutlineView pov) {
					if (pov.SelectedRow != -1) {
						if (pov.ItemAtRow (pov.SelectedRow) is NSObjectFacade facade) {
							switch (facade.Target) {
							case PropertyTreeElement propertyTreeElement:
								viewModel.SelectedPropertyElement = propertyTreeElement;
								break;

							default:
								break;
							}
							SetCustomPath (viewModel);
						}
					}
				}
			};

			var pathColumn = new NSTableColumn (PathSelectorColumnColId);
			this.pathOutlineView.AddColumn (pathColumn);

			// Set OutlineTableColumn or the arrows showing children/expansion will not be drawn
			this.pathOutlineView.OutlineTableColumn = pathColumn;

			// add the panel to the window
			outlineViewContainer.DocumentView = this.pathOutlineView;
			AddSubview (outlineViewContainer);

			AddConstraints (new[] {
				outlineViewContainerTopConstraint,
				NSLayoutConstraint.Create (outlineViewContainer, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this, NSLayoutAttribute.Width, 1f, -10f),
				NSLayoutConstraint.Create (outlineViewContainer, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, this, NSLayoutAttribute.CenterX, 1, 0),
				NSLayoutConstraint.Create (outlineViewContainer, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal,this, NSLayoutAttribute.Bottom, 1f, -5f),
			});

			viewModel.PropertyChanged += async (sender, e) => {
				if (viewModel.PropertyRoot != null) {
					this.pathOutlineView.PropertyTreeRoot = await viewModel.PropertyRoot.Task;
				} else {
					this.pathOutlineView.PropertyTreeRoot = null;
				}
			};
		}

		private void SetCustomPath (CreateBindingViewModel viewModel)
		{
			this.customPath.StringValue = this.customPath.Enabled ? viewModel.Path ?? string.Empty : string.Empty;
		}
	}
}
