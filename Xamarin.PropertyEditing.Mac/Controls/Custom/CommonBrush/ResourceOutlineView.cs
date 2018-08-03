using System;
using AppKit;
using Foundation;
using Xamarin.PropertyEditing.Mac.Resources;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class ResourceOutlineView : NSOutlineView
	{
		internal const string ResourcePreviewColId = "Preview";
		internal const string ResourceNameColId = "Name";

		public ResourceOutlineView ()
		{
			Initialize ();
		}

		// Called when created from unmanaged code
		public ResourceOutlineView (IntPtr handle) : base (handle)
		{
		}

		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public ResourceOutlineView (NSCoder coder) : base (coder)
		{
		}

		public void Initialize ()
		{
			var nameColumn = new NSTableColumn (ResourceNameColId) {
				Title = LocalizationResources.ColumnResourceName,
				Width = 150,
			};
			AddColumn (nameColumn);
			var previewColumn = new NSTableColumn (ResourcePreviewColId) {
				Title = LocalizationResources.ColumnResourcePreview,
				Width = 150,
			};
			AddColumn (previewColumn);
		}

		[Export ("validateProposedFirstResponder:forEvent:")]
		public bool validateProposedFirstResponder (NSResponder responder, NSEvent ev)
		{
			return true;
		}

		private ResourceSelectorViewModel viewModel;
		public ResourceSelectorViewModel ViewModel
		{
			get => this.viewModel;
			set
			{
				this.viewModel = value;
				DataSource = new ResourceDataSource (viewModel);
			}
		}
	}
}
