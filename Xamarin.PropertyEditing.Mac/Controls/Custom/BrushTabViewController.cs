using System;
using System.Linq;
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

			this.filterResource = new NSSearchField {
				ControlSize = NSControlSize.Mini,
				Font = NSFont.FromFontName (PropertyEditorControl.DefaultFontName, PropertyEditorControl.DefaultFontSize),
				PlaceholderString = Properties.Resources.SearchResourcesTitle,
			};

			this.filterResource.Changed += (sender, e) => {
				ViewModel.ResourceSelector.FilterText = this.filterResource.Cell.Title;
				this.resource.ReloadData ();
			};

			this.filterResource.Hidden = true;

			TabStack.AddView (this.filterResource, NSStackViewGravity.Leading);
		}

		public override void OnViewModelChanged (BrushPropertyViewModel oldModel)
		{
			this.inhibitSelection = true;
			base.OnViewModelChanged (oldModel);

			var existing = new HashSet<CommonBrushType> (ViewModel?.BrushTypes?.Values ?? Array.Empty<CommonBrushType> ());
			existing.IntersectWith (this.brushTypeTable.Keys);

			var removed = new HashSet<CommonBrushType> (this.brushTypeTable.Keys);
			removed.ExceptWith (existing);

			foreach (var item in removed.Select (t => new { Type = t, Tab = TabView.Items[this.brushTypeTable[t]] }).ToArray ()) {
				RemoveTabViewItem (item.Tab);
				item.Tab.Dispose ();
				this.brushTypeTable.Remove (item.Type);
			}

			if (ViewModel == null)
				return;

			int i = -1;
			foreach (var kvp in ViewModel.BrushTypes) {
				i++;
				this.brushTypeTable[kvp.Value] = i;
				if (existing.Contains (kvp.Value)) {
					((NotifyingViewController<BrushPropertyViewModel>)TabViewItems[i].ViewController).ViewModel = ViewModel;
					continue;
				}

				var item = new NSTabViewItem ();
				item.Label = kvp.Key;

				switch (kvp.Value) {
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
						this.resource = new ResourceBrushViewController ();
						this.resource.ViewModel = ViewModel;
						item.ViewController = this.resource;
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
					default:
					case CommonBrushType.NoBrush:
						var none = new EmptyBrushEditorViewController ();
						none.ViewModel = ViewModel;
						item.ViewController = none;
						item.ToolTip = Properties.Resources.NoBrush;
						item.Image = NSImage.ImageNamed ("property-brush-none-16");
						break;
				}

				InsertTabViewItem (item, i);
			}

			if (this.brushTypeTable.TryGetValue (ViewModel.SelectedBrushType, out int index)) {
				SelectedTabViewItemIndex = index;
			}

			this.inhibitSelection = false;
		}

		public override void OnPropertyChanged (object sender, PropertyChangedEventArgs args)
		{
			base.OnPropertyChanged (sender, args);
			switch (args.PropertyName) {
				case nameof (BrushPropertyViewModel.SelectedBrushType):
					if (this.brushTypeTable.TryGetValue (ViewModel.SelectedBrushType, out int index)) {
						SelectedTabViewItemIndex = index;
					}
					break;
			}
		}

		public override void WillSelect (NSTabView tabView, NSTabViewItem item)
		{
			if (item.ViewController is NotifyingViewController<BrushPropertyViewModel> brushController)
				brushController.ViewModel = ViewModel;

			if (this.inhibitSelection)
				return;

			base.WillSelect (tabView, item);
		}

		public override void DidSelect (NSTabView tabView, NSTabViewItem item)
		{
			if (this.inhibitSelection)
				return;

			ViewModel.SelectedBrushType = ViewModel.BrushTypes[item.Label];
			this.filterResource.Hidden = ViewModel.SelectedBrushType != CommonBrushType.Resource;

			base.DidSelect (tabView, item);
		}

		public override void ViewDidLoad ()
		{
			View.Frame = new CGRect (0, 0, 430, 230);

			this.inhibitSelection = true;
			base.ViewDidLoad ();
			this.inhibitSelection = false;
		}

		private readonly Dictionary<CommonBrushType, int> brushTypeTable = new Dictionary<CommonBrushType, int> ();
		private bool inhibitSelection;

		private NSSearchField filterResource;
		private ResourceBrushViewController resource;
	}
}
