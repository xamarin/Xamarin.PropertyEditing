using System;
using AppKit;
using CoreGraphics;
using Foundation;
using Xamarin.PropertyEditing.Drawing;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class ResourceOutlineViewDelegate : NSOutlineViewDelegate
	{
		private const string labelIdentifier = "label";
		private const string resourceIdentifier = "resource";

		public override NSView GetView (NSOutlineView outlineView, NSTableColumn tableColumn, NSObject item)
		{
			var facade = item as NSObjectFacade;
			var resource = facade?.Target as Resource;
			switch (tableColumn.Identifier) {
				case ResourceOutlineView.ResourcePreviewColId:
					var cbv = (CommonBrushView)outlineView.MakeView (resourceIdentifier, this);
					if (cbv == null) {
						cbv = new CommonBrushView {
							Identifier = resourceIdentifier,
							Frame = new CGRect (0, 0, 30, 10),
						};
					}

					var commonBrush = BrushPropertyViewModel.GetCommonBrushForResource (resource);
					if (commonBrush != null)
						cbv.Brush = commonBrush;

					return cbv;

				case ResourceOutlineView.ResourceNameColId:
				default:
					var utf = (UnfocusableTextField)outlineView.MakeView (labelIdentifier, this);
					if (utf == null) {
						utf = new UnfocusableTextField {
							Identifier = labelIdentifier,
						};
					}
					utf.StringValue = resource.Name;
					return utf;
			}
		}


		public override nfloat GetRowHeight (NSOutlineView outlineView, NSObject item)
		{
			return PropertyEditorControl.DefaultControlHeight;
		}
	}
}
