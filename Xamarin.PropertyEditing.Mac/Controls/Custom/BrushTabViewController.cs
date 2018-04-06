using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using AppKit;
using CoreAnimation;
using CoreGraphics;
using Xamarin.PropertyEditing.Drawing;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	class MaterialBrushEditorViewController : PropertyViewController <BrushPropertyViewModel>
	{
		MaterialView materialEditor;

		public MaterialBrushEditorViewController ()
		{
			PreferredContentSize = new CGSize (100, 100);
		}

		protected override void OnPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName) {
				case nameof (BrushPropertyViewModel.Solid):
					if (materialEditor != null)
						materialEditor.ViewModel = ViewModel;
					break;

			}
		}

		protected override void OnViewModelChanged (BrushPropertyViewModel oldModel)
		{
			if (materialEditor != null)
				materialEditor.ViewModel = ViewModel;
		}

		public class MaterialView : NSView
		{
			public MaterialView () : base ()
			{

			}

			void Initialize () {
				WantsLayer = true;
				Layer = new CALayer {
					CornerRadius = 3,
					BorderColor = NSColor.SystemGrayColor.CGColor,
					BorderWidth = 1
				};
			}

			BrushPropertyViewModel viewModel;
			public BrushPropertyViewModel ViewModel
			{
				get => viewModel;
				set {
					viewModel = value;
					NeedsLayout = true;
				}
			}

			public MaterialDesignColorViewModel MaterialDesign {
				get => ViewModel?.MaterialDesign;
			}

			public override void Layout ()
			{
				if (ViewModel.MaterialDesign != null) {
					Layer.BackgroundColor = (MaterialDesign?.AccentColor ?? CommonColor.White).ToCGColor ();
				}
				base.Layout ();
			}
        }

		public override void LoadView ()
		{
			View = materialEditor = new MaterialView ();
		}
	}

	class EmptyBrushEditorViewController : NSViewController
	{
		NSButton brushEditor;

		public EmptyBrushEditorViewController ()
		{
			PreferredContentSize = new CGSize (100, 100);
		}

		BrushPropertyViewModel viewModel;
		internal BrushPropertyViewModel ViewModel
		{
			get => viewModel;
			set
			{
				viewModel = value;
				//if (brushEditor != null)
					//brushEditor.ViewModel = viewModel?.Solid;
			}
		}

		public override void LoadView ()
		{
			View = brushEditor = new NSButton {
				StringValue = "taco"
			};
			brushEditor.Activated += (o, e) => {
				ViewModel.SelectedBrushType = CommonBrushType.Solid;
			};
		}
	}

	class BrushTabViewController : PropertyTabViewController<BrushPropertyViewModel>
	{
		Dictionary<CommonBrushType, int> BrushTypeTable = new Dictionary<CommonBrushType, int> ();

		bool inhibitSelection;

		protected override void OnViewModelChanged (BrushPropertyViewModel oldModel)
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
						var material2 = new EmptyBrushEditorViewController ();
						material2.ViewModel = ViewModel;
						item.ViewController = material2;
						break;
					case CommonBrushType.Gradient:
						var material3 = new EmptyBrushEditorViewController ();
						material3.ViewModel = ViewModel;
						item.ViewController = material3;
						break;
					case CommonBrushType.NoBrush:
						var material4 = new EmptyBrushEditorViewController ();
						material4.ViewModel = ViewModel;
						item.ViewController = material4;
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

		protected override void OnPropertyChanged (object sender, PropertyChangedEventArgs args)
		{
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
            base.WillSelect(tabView, item);
        }

        public override void DidSelect(NSTabView tabView, NSTabViewItem item)
        {
            base.DidSelect(tabView, item);

			if (inhibitSelection)
				return;
			ViewModel.SelectedBrushType = ViewModel.BrushTypes[item.Label];
        }
    }
}
