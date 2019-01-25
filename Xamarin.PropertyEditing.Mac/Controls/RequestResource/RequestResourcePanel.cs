using System;
using System.ComponentModel;
using AppKit;
using CoreGraphics;
using Xamarin.PropertyEditing.Drawing;
using Xamarin.PropertyEditing.Mac.Resources;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class RequestResourcePanel : NSView
	{
		internal const string ResourceImageColId = "ResourceImage";
		internal const string ResourceTypeColId = "ResourceType";
		internal const string ResourceNameColId = "ResourceName";
		internal const string ResourceValueColId = "ResourceValue";

		private NSTableView resourceTable;
		private ResourceTableDataSource dataSource;

		private readonly ResourceSelectorViewModel viewModel;

		public ResourceSelectorViewModel ViewModel => this.viewModel;
		private SimpleCollectionView collectionView => this.viewModel.Resources as SimpleCollectionView;
		public Resource SelectedResource {
			get {
				return (this.resourceTable.SelectedRow != -1) ? this.collectionView [((int)this.resourceTable.SelectedRow)] as Resource : null;
			}
		}

		NSProgressIndicator progressIndicator;

		NSScrollView tableContainer;

		RequestResourcePreviewPanel previewPanel;

		public event EventHandler ResourceSelected;
		public event EventHandler DoubleClicked;

		private object selectedValue;

		public RequestResourcePanel (IHostResourceProvider hostResources, ResourceSelectorViewModel viewModel, object value)
		{
			if (hostResources == null)
				throw new ArgumentNullException (nameof (hostResources));
				
			this.viewModel = viewModel;
			this.viewModel.PropertyChanged += OnPropertyChanged;
			Initialize (hostResources, value);
		}

		private void OnPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof (this.viewModel.IsLoading)) {
				this.progressIndicator.Hidden = !this.viewModel.IsLoading;
				this.tableContainer.Hidden = !this.progressIndicator.Hidden;
				if (this.viewModel.IsLoading) {
					this.progressIndicator.StartAnimation (null);
				} else {
					this.progressIndicator.StopAnimation (null);
				}
			}
		}

		private void Initialize (IHostResourceProvider hostResources, object selectedValue)
		{
			this.selectedValue = selectedValue;
			Frame = new CGRect (10, 35, 630, 305);

			var FrameHeightHalf = (Frame.Height - 32) / 2;
			var FrameWidthHalf = (Frame.Width - 32) / 2;
			var FrameWidthThird = (Frame.Width - 32) / 3;

			this.progressIndicator = new NSProgressIndicator (new CGRect (FrameWidthThird, FrameHeightHalf, 32, 32)) {
				Hidden = true,
				Style = NSProgressIndicatorStyle.Spinning,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};
			AddSubview (this.progressIndicator);

			this.resourceTable = new FirstResponderTableView {
				AutoresizingMask = NSViewResizingMask.WidthSizable,
				HeaderView = null,
			};

			this.dataSource = new ResourceTableDataSource (viewModel);
			var resourceTableDelegate = new ResourceTableDelegate (hostResources, dataSource);
			resourceTableDelegate.ResourceSelected += (sender, e) => {
				this.previewPanel.SelectedResource = SelectedResource;

				this.selectedValue = BrushPropertyViewModel.GetCommonBrushForResource (SelectedResource);

				ResourceSelected?.Invoke (this, EventArgs.Empty);
			};
			this.resourceTable.Delegate = resourceTableDelegate;
			this.resourceTable.DataSource = dataSource;

			NSTableColumn resourceImages = new NSTableColumn (ResourceImageColId) { Title = LocalizationResources.ResourceTableImageColumnTitle, Width = 32 };
			this.resourceTable.AddColumn (resourceImages);

			NSTableColumn resourceTypes = new NSTableColumn (ResourceTypeColId) { Title = LocalizationResources.ResourceTableTypeColumnTitle, Width = 150 };
			this.resourceTable.AddColumn (resourceTypes);

			NSTableColumn resourceName = new NSTableColumn (ResourceNameColId) { Title = LocalizationResources.ResourceTableNameColumnTitle, Width = 150 };
			resourceTable.AddColumn (resourceName);

			NSTableColumn resourceValue = new NSTableColumn (ResourceValueColId) { Title = LocalizationResources.ResourceTableValueColumnTitle, Width = 45 };
			this.resourceTable.AddColumn (resourceValue);

			this.resourceTable.DoubleClick += (object sender, EventArgs e) => {
				DoubleClicked?.Invoke (this, EventArgs.Empty);
			};

			// create a table view and a scroll view
			this.tableContainer = new NSScrollView (new CGRect (0, 0, resourceTable.TableColumns ()[0].Width + resourceTable.TableColumns ()[1].Width + resourceTable.TableColumns ()[2].Width + 10, Frame.Height)) {
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			this.tableContainer.DocumentView = resourceTable;
			AddSubview (this.tableContainer);

			this.previewPanel = new RequestResourcePreviewPanel (hostResources, new CGRect (Frame.Width - FrameWidthThird, 0, FrameWidthThird, Frame.Height));
			AddSubview (this.previewPanel);

			this.AddConstraints (new[] {
				NSLayoutConstraint.Create (this.tableContainer, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Width, 1f, -190f),
				NSLayoutConstraint.Create (this.tableContainer, NSLayoutAttribute.Height, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Height, 1f, 0f),
			});

			ReloadData ();
		}

		internal void ReloadData ()
		{
			this.resourceTable.ReloadData ();

			if (collectionView.Count > 0 && this.selectedValue != null) {
				for (int i = 0; i < collectionView.Count; i++) {
					var element = collectionView[i] as Resource;
					var eType = element.GetType ();
					var valuePropertyInfo = eType.GetProperty ("Value");
					var elementValue = valuePropertyInfo.GetValue (element);
					if (elementValue == this.selectedValue) {
						this.resourceTable.SelectRow (i, false);
						break;
					}
				}
			}
		}
	}
}
