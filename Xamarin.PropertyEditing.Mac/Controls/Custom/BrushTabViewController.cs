using System.Collections.Generic;
using System.ComponentModel;
using AppKit;
using CoreGraphics;
using Foundation;
using Xamarin.PropertyEditing.Drawing;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	class BrushTabViewController : PropertyTabViewController<BrushPropertyViewModel>
	{
		public BrushTabViewController ()
		{
			PreferredContentSize = new CGSize (300, 230);
		}

		Dictionary<CommonBrushType, int> BrushTypeTable = new Dictionary<CommonBrushType, int> ();
		bool inhibitSelection;

		public override void OnViewModelChanged (BrushPropertyViewModel oldModel)
        {
			inhibitSelection = true;
            base.OnViewModelChanged(oldModel);

			foreach (var item in TabViewItems) {
				RemoveTabViewItem (item);
			}

			BrushTypeTable.Clear ();
			if (ViewModel == null)
				return;

			foreach (var key in ViewModel.BrushTypes.Keys) {
				var item = new NSTabViewItem ();
				item.Label = key;
				var brushType = ViewModel.BrushTypes[key];

				switch (brushType) {
					case CommonBrushType.Solid:
						var solid = new SolidColorBrushEditorViewController ();
						solid.ViewModel = ViewModel;
						item.ViewController = solid;
						break;
					case CommonBrushType.MaterialDesign:
						var material = new MaterialBrushEditorViewController ();
						material.ViewModel = ViewModel;
						item.ViewController = material;
						break;
					case CommonBrushType.Resource:
						var resource = new ResourceBrushViewController ();
						resource.ViewModel = ViewModel;
						item.ViewController = resource;
						break;
					case CommonBrushType.Gradient:
						var gradient = new EmptyBrushEditorViewController ();
						gradient.ViewModel = ViewModel;
						item.ViewController = gradient;
						break;
					case CommonBrushType.NoBrush:
						var none = new EmptyBrushEditorViewController ();
						none.ViewModel = ViewModel;
						item.ViewController = none;
						break;

				}
				if (item.ViewController != null) {
					BrushTypeTable[brushType] = TabViewItems.Length;
					AddTabViewItem (item);
				}
			}

			if (BrushTypeTable.TryGetValue (ViewModel.SelectedBrushType, out var index)) {
				SelectedTabViewItemIndex = index;
			}
			inhibitSelection = false;
        }

		public override void OnPropertyChanged (object sender, PropertyChangedEventArgs args)
		{
			base.OnPropertyChanged (sender, args);
			switch (args.PropertyName) {
				case nameof (BrushPropertyViewModel.SelectedBrushType):
					if (BrushTypeTable.TryGetValue (ViewModel.SelectedBrushType, out var index)) {
						this.SelectedTabViewItemIndex = index;
					}
					break;
			}
		}

        public override void WillSelect(NSTabView tabView, NSTabViewItem item)
        {
			var brushController = item.ViewController as PropertyViewController<BrushPropertyViewModel>;
			if (brushController != null)
				brushController.ViewModel = ViewModel;

			if (inhibitSelection)
				return;

            base.WillSelect(tabView, item);
        }

        public override void DidSelect(NSTabView tabView, NSTabViewItem item)
        {
			if (inhibitSelection)
				return;

			base.DidSelect (tabView, item);
			ViewModel.SelectedBrushType = ViewModel.BrushTypes[item.Label];
        }

        public override void ViewDidLoad()
        {
			var old = View.Frame;
			old.Height = 230;
			View.Frame = old;

			inhibitSelection = true;
            base.ViewDidLoad();
			inhibitSelection = false;

			old = View.Frame;
			//old.Height = 230;
			View.Frame = old;
        }
    }
}
