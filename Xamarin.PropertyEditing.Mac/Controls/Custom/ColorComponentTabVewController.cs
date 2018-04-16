using System;
using System.ComponentModel;
using AppKit;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	class ColorComponentTabViewController : PropertyTabViewController<SolidBrushViewModel>
	{
		public EditorType EditorType { get; set; }

		public ColorComponentTabViewController ()
		{
			foreach (var value in Enum.GetValues (typeof (EditorType))) {
				var editorType = (EditorType)value;
				AddTabViewItem (new NSTabViewItem {
					Label = value.ToString (),
					ViewController = new ColorComponentViewController (editorType)
				});
			}
		}

        protected override void OnViewModelChanged(SolidBrushViewModel oldModel)
        {
            base.OnViewModelChanged(oldModel);
			var controller = TabView.Item(SelectedTabViewItemIndex).ViewController as ColorComponentViewController;
			controller.ViewModel = ViewModel;
        }

        public override void WillSelect (NSTabView tabView, NSTabViewItem item)
		{
			var controller = item.ViewController as ColorComponentViewController;

			base.WillSelect (tabView, item);
			controller.ViewModel = ViewModel;
			this.EditorType = controller.EditorType;
		}

		public override void DidSelect (NSTabView tabView, NSTabViewItem item)
		{
			base.DidSelect (tabView, item);
			var controller = item.ViewController as ColorComponentViewController;
			this.EditorType = controller.EditorType;
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			//View.Appearance = PropertyEditorPanel.ThemeManager.CurrentAppearance;
			var old = View.Frame;
			old.Height = 200;
			View.Frame = old;
		}
	}

}
