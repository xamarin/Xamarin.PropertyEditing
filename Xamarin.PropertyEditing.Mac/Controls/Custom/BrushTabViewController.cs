using System.Collections.Generic;
using System.ComponentModel;
using AppKit;
using CoreGraphics;
using Foundation;
using Xamarin.PropertyEditing.Drawing;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class BrushTabViewController : UnderlinedTabViewController<BrushPropertyViewModel>
	{
		public BrushTabViewController ()
		{
			PreferredContentSize = new CGSize (430, 230);
			TransitionOptions = NSViewControllerTransitionOptions.None;
			EdgeInsets = new NSEdgeInsets (0, 12, 12, 12);
		}

		Dictionary<CommonBrushType, int> BrushTypeTable = new Dictionary<CommonBrushType, int> ();
		bool inhibitSelection;

		public override void OnViewModelChanged (BrushPropertyViewModel oldModel)
		{
			this.inhibitSelection = true;
			base.OnViewModelChanged (oldModel);

			foreach (var item in TabViewItems) {
				RemoveTabViewItem (item);
				item.Dispose ();
			}

			BrushTypeTable.Clear ();
			if (ViewModel == null)
				return;

			foreach (var key in ViewModel.BrushTypes.Keys) {
				var item = new NSTabViewItem ();
				item.Label = key;
				var brushType = ViewModel.BrushTypes [key];

				switch (brushType) {
					case CommonBrushType.Solid:
						var solid = new SolidColorBrushEditorViewController ();
						solid.ViewModel = ViewModel;
						item.ViewController = solid;
						item.ToolTip = Properties.Resources.SolidBrush;
						item.Image = NSImage.ImageNamed ("property-brush-solid-16");
						break;
					case CommonBrushType.MaterialDesign:
						var material = new MaterialBrushEditorViewController ();
						material.ViewModel = ViewModel;
						item.ViewController = material;
						item.ToolTip = Properties.Resources.MaterialDesignColorBrush;
						item.Image = NSImage.ImageNamed ("property-brush-palette-16");
						break;
					case CommonBrushType.Resource:
						var resource = new ResourceBrushViewController ();
						resource.ViewModel = ViewModel;
						item.ViewController = resource;
						item.ToolTip = Properties.Resources.ResourceBrush;
						item.Image = NSImage.ImageNamed ("property-brush-resources-16");
						break;
					case CommonBrushType.Gradient:
						var gradient = new EmptyBrushEditorViewController ();
						gradient.ViewModel = ViewModel;
						item.ViewController = gradient;
						item.ToolTip = item.Label;
						item.Image = NSImage.ImageNamed ("property-brush-gradient-16");
						break;
					case CommonBrushType.NoBrush:
						var none = new EmptyBrushEditorViewController ();
						none.ViewModel = ViewModel;
						item.ViewController = none;
						item.ToolTip = Properties.Resources.NoBrush;
						item.Image = NSImage.ImageNamed ("property-brush-none-16");
						break;
				}
				if (item.ViewController != null) {
					BrushTypeTable [brushType] = TabViewItems.Length;
					AddTabViewItem (item);
				}
			}

			if (BrushTypeTable.TryGetValue (ViewModel.SelectedBrushType, out var index)) {
				SelectedTabViewItemIndex = index;
			}
			this.inhibitSelection = false;
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

		public override void WillSelect (NSTabView tabView, NSTabViewItem item)
		{
			var brushController = item.ViewController as NotifyingViewController<BrushPropertyViewModel>;
			if (brushController != null)
				brushController.ViewModel = ViewModel;
			
			if (this.inhibitSelection)
				return;

			base.WillSelect (tabView, item);
		}

		public override void DidSelect (NSTabView tabView, NSTabViewItem item)
		{
			if (this.inhibitSelection)
				return;

			base.DidSelect (tabView, item);
			ViewModel.SelectedBrushType = ViewModel.BrushTypes [item.Label];
		}

		public override void ViewDidLoad ()
		{
			View.Frame = new CGRect (0, 0, 430,230);

			this.inhibitSelection = true;
			base.ViewDidLoad ();
			this.inhibitSelection = false;
		}
	}
}
