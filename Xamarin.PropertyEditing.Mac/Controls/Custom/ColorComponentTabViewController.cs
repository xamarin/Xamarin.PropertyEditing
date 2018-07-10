using System;
using System.ComponentModel;
using AppKit;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class ColorComponentTabViewController : NotifyingTabViewController<SolidBrushViewModel>
	{
		public ColorComponentTabViewController ()
		{
			foreach (var value in Enum.GetValues (typeof (ChannelEditorType))) {
				var editorType = (ChannelEditorType)value;
				AddTabViewItem (new NSTabViewItem {
					Label = value.ToString (),
					ViewController = new ColorComponentViewController (editorType)
				});
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
