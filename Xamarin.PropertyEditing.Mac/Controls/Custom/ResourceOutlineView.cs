using System;
using AppKit;
using CoreGraphics;
using Foundation;
using ObjCRuntime;
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
			RowHeight = 24;
		}

		// Called when created from unmanaged code
		public ResourceOutlineView (NativeHandle handle) : base (handle)
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
				Title = Properties.Resources.ColumnResourceName,
				Width = 150,
			};
			AddColumn (nameColumn);

			var previewColumn = new NSTableColumn (ResourcePreviewColId) {
				Title = Properties.Resources.ColumnResourcePreview,
				Width = 150,
			};
			AddColumn (previewColumn);

			HeaderView = null;
		}

		public override bool ValidateProposedFirstResponder (NSResponder responder, NSEvent forEvent)
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
