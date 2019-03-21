using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AppKit;
using Foundation;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class PathOutlineView : BaseSelectorOutlineView
	{
		private IReadOnlyCollection<object> itemsSource;
		private PropertyTreeRoot propertyTreeRoot;

		private void SetItemsSource (IReadOnlyCollection<object> value, string targetName)
		{
			if (this.itemsSource != value) {
				this.itemsSource = value;

				DataSource = new PathOutlineViewDataSource (this.itemsSource, targetName); ;
				Delegate = new PathOutlineViewDelegate ();

				ReloadData ();

				ExpandItem (ItemAtRow (0));
			}
		}

		public PropertyTreeRoot PropertyTreeRoot
		{
			get { return this.propertyTreeRoot; }
			internal set {
				if (this.propertyTreeRoot != value) {
					this.propertyTreeRoot = value;
					if (this.propertyTreeRoot != null) {
						SetItemsSource (this.propertyTreeRoot.Children, this.propertyTreeRoot.TargetType.Name);
					} else {
						SetItemsSource (null, string.Empty);
					}
				}
			}
		}
	}

	internal class PathOutlineViewDelegate : BaseOutlineViewDelegate
	{
		private const string PathIdentifier = "path";

		public override NSView GetView (NSOutlineView outlineView, NSTableColumn tableColumn, NSObject item)
		{
			var labelContainer = (UnfocusableTextField)outlineView.MakeView (PathIdentifier, this);
			if (labelContainer == null) {
				labelContainer = new UnfocusableTextField {
					Identifier = PathIdentifier,
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
					labelContainer.StringValue = Properties.Resources.TypeNotSupported;
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
		private readonly IReadOnlyCollection<object> itemsSource;
		private readonly string targetName;

		internal PathOutlineViewDataSource (IReadOnlyCollection<object> itemsSource, string targetName)
		{
			this.itemsSource = itemsSource;
			this.targetName = targetName;
		}

		public override nint GetChildrenCount (NSOutlineView outlineView, NSObject item)
		{
			if (item == null) {
				return this.itemsSource != null ? this.itemsSource.Count + 1 : 0;
			} else {
				var target = (item as NSObjectFacade).Target;
				switch (target) {
					case PropertyTreeElement propertyTreeElement:
						IReadOnlyCollection<PropertyTreeElement> propertyTrees = propertyTreeElement.Children.Task.Result;
						return propertyTrees.Count;

					case string targetName:
						return this.itemsSource.Count;

					default:
						return 0;
				}
			}
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
						return new NSObjectFacade (this.itemsSource.ElementAt ((int)childIndex));

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

        public NSTextField CustomPath { get; }

		private readonly CreateBindingViewModel viewModel;

		public BindingPathSelectorControl (CreateBindingViewModel viewModel)
		{
			if (viewModel == null)
				throw new ArgumentNullException (nameof (viewModel));

			this.viewModel = viewModel;

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
				NSLayoutConstraint.Create (customCheckBox, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this, NSLayoutAttribute.Top, 1f, 3f),
				NSLayoutConstraint.Create (customCheckBox, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this, NSLayoutAttribute.Right, 1f, -90f),
			});

			this.CustomPath = new NSTextField {
				ControlSize = NSControlSize.Small,
				Enabled = false,
				Font = NSFont.SystemFontOfSize (NSFont.SystemFontSizeForControlSize (NSControlSize.Small)),
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			var customPathHeightConstraint = NSLayoutConstraint.Create (this.CustomPath, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 0);

			AddSubview (this.CustomPath);
			AddConstraints (new[] {
				NSLayoutConstraint.Create (this.CustomPath, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this, NSLayoutAttribute.Top, 1f, 35f),
				NSLayoutConstraint.Create (this.CustomPath, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this, NSLayoutAttribute.Width, 1f, -10f),
				NSLayoutConstraint.Create (this.CustomPath, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, this, NSLayoutAttribute.CenterX, 1, 0),
				customPathHeightConstraint,
			});

			// create a table view and a scroll view
			var outlineViewContainer = new NSScrollView {
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			var outlineViewContainerTopConstraint = NSLayoutConstraint.Create (outlineViewContainer, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.CustomPath, NSLayoutAttribute.Bottom, 1f, 0f);
			customCheckBox.Activated += (sender, e) => {
				this.CustomPath.Enabled = customCheckBox.State == NSCellStateValue.On;
				customPathHeightConstraint.Constant = this.CustomPath.Enabled ? 20 : 0;
				outlineViewContainerTopConstraint.Constant = this.CustomPath.Enabled ? 10 : 0;
			};

			this.CustomPath.Changed += (sender, e) => {
				viewModel.Path = this.CustomPath.StringValue;
			};

			this.pathOutlineView = new PathOutlineView {

			};

			this.pathOutlineView.Activated += OnPathOutlineViewSelected;

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

			viewModel.PropertyChanged += OnPropertyChanged;
		}

		private async void OnPropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof (CreateBindingViewModel.PropertyRoot)) {
				if (this.viewModel.PropertyRoot != null) {
					this.pathOutlineView.PropertyTreeRoot = await this.viewModel.PropertyRoot.Task;
				} else {
					this.pathOutlineView.PropertyTreeRoot = null;
				}
			}

			if (e.PropertyName == nameof (CreateBindingViewModel.Path)) {
				this.CustomPath.StringValue = this.viewModel.Path ?? string.Empty;
			}
		}

		private void OnPathOutlineViewSelected (object sender, EventArgs e)
		{
			if (sender is PathOutlineView pov) {
				if (pov.SelectedRow != -1) {
					if (pov.ItemAtRow (pov.SelectedRow) is NSObjectFacade facade) {
						switch (facade.Target) {
						case PropertyTreeElement propertyTreeElement:
							this.viewModel.SelectedPropertyElement = propertyTreeElement;
							break;

						default:
							break;
						}
					}
				}
			}
		}

	}
}
