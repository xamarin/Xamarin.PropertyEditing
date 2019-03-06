using System;
using System.Collections.Generic;
using System.ComponentModel;
using AppKit;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class ColorComponentTabViewController
		: UnderlinedTabViewController<SolidBrushViewModel>
	{
		public ColorComponentTabViewController (IHostResourceProvider hostResources)
			: base (hostResources)
		{
			foreach (var value in Enum.GetValues (typeof (ChannelEditorType))) {
				var editorType = (ChannelEditorType)value;
				AddTabViewItem (new NSTabViewItem {
					Label = value.ToString (),
					ToolTip = GetToolTip (editorType),
					ViewController = new ColorComponentViewController (hostResources, editorType)
				});
			}

			ContentPadding = new NSEdgeInsets (9, 0, 9, 0);
			TabStack.Spacing = 4;
		}

		private string GetToolTip (ChannelEditorType editorType)
		{
			switch (editorType) {
			case ChannelEditorType.CMYK:
				return Properties.Resources.CMYK;
			case ChannelEditorType.HLS:
				return Properties.Resources.HLS;
			case ChannelEditorType.HSB:
				return Properties.Resources.HSB;
			case ChannelEditorType.RGB:
				return Properties.Resources.RGB;
			default:
				return String.Empty;
			}
		}

		public ChannelEditorType EditorType { get; set; }

		public override void OnViewModelChanged (SolidBrushViewModel oldModel)
		{
			base.OnViewModelChanged (oldModel);
			if (ViewLoaded) {
				var controller = TabView.Item (SelectedTabViewItemIndex).ViewController as ColorComponentViewController;
				controller.ViewModel = ViewModel;
			}
		}

		public override void WillSelect (NSTabView tabView, NSTabViewItem item)
		{
			var controller = item.ViewController as ColorComponentViewController;

			base.WillSelect (tabView, item);

			controller.ViewModel = ViewModel;
			EditorType = controller.EditorType;
		}

		public override void DidSelect (NSTabView tabView, NSTabViewItem item)
		{
			base.DidSelect (tabView, item);
			var controller = item.ViewController as ColorComponentViewController;
			EditorType = controller.EditorType;
		}
	}
}
