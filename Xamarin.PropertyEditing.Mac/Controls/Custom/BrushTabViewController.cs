using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using AppKit;
using CoreAnimation;
using CoreGraphics;
using Foundation;
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
				case nameof (BrushPropertyViewModel.MaterialDesign):
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
			public override bool IsFlipped => true;

			readonly string[] ColorNames = {
				"50",
				"100",
				"200",
				"300",
				"400",
				"500",
				"600",
				"700",
				"800",
				"900"
			};

			readonly string[] AccentNames = {
				"A100",
				"A200",
				"A400",
				"A700"
			};

			readonly string[] BlackWhite = {
				"White",
				"Black"
			};

			public MaterialView () : base ()
			{
				Initialize ();
			}

			void Initialize () {
				WantsLayer = true;
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

			CGRect frame;
			String name;
            public override void Layout()
			{
				base.Layout ();
				if (name == MaterialDesign.ColorName && frame == Frame) {
					return;
				}
				name = MaterialDesign.ColorName;
				frame = Frame;

				if (Subviews != null)
					foreach (var v in this.Subviews)
						v.RemoveFromSuperview ();

				if (Layer?.Sublayers != null)
					foreach (var l in Layer.Sublayers)
						l.RemoveFromSuperLayer ();

				if (MaterialDesign != null) {
					var colors = MaterialDesign.Palettes.Select (p => new { p.Name, Color = p.MainColor }).ToArray ();
					int col = 0;
					nfloat x = 0;
					nfloat y = 0;
					var width = Frame.Width / 10;
					var height = Frame.Height / 5;

					foreach (var p in colors) {
						var frame = new CGRect (x, y, width, height);
						var l = new CALayer {
							Frame = frame,
							BackgroundColor = p.Color.ToCGColor (),
							CornerRadius = 3,
							BorderColor = NSColor.SelectedControl.CGColor,
							BorderWidth = MaterialDesign.ColorName == p.Name ? 3 : 0
						};
						
						Layer.AddSublayer (l);
						x += width;
						col++;
						if (col >= 10) {
							x = 0;
							y += height;
							col = 0;
						}
					}

					AddSubview (new NSTextField (new CGRect (x, y, Frame.Width, 30))
					{
						StringValue = MaterialDesign.ColorName,
						DrawsBackground = false,
						Bordered = false,
						Editable = false
					});
					y += 30;
					x = 0;
					width = Frame.Width / MaterialDesign.NormalColorScale.Count ();
					var names = MaterialDesign.NormalColorScale.Count () > 2 ? ColorNames : BlackWhite;
					foreach (var n in MaterialDesign.NormalColorScale.Zip (names, (c, n) => new { Name = n, Color = c.Value })) {
						var frame = new CGRect (x, y, width, height);
						var l = new CALayer {
							BackgroundColor = n.Color.ToCGColor (),
							CornerRadius = 3,
							Frame = frame
						};
						Layer.AddSublayer (l);
						var c = new NSTextField (frame) {
							StringValue = n.Name,
							BackgroundColor = NSColor.Clear,
							DrawsBackground = false,
							Bordered = false,
							Editable = false,
						};
						AddSubview (c);
						x += width;
					}

					y += height;
					x = 0;
					width = Frame.Width / MaterialDesign.AccentColorScale.Count ();
					foreach (var n in MaterialDesign.AccentColorScale.Zip (AccentNames, (c, n) => new { Name = n, Color = c.Value })) {
						var frame = new CGRect (x, y, width, height);
						var l = new CALayer {
							BackgroundColor = n.Color.ToCGColor (),
							CornerRadius = 3,
							Frame = frame
						};
						Layer.AddSublayer (l);
						var c = new NSTextField (frame) {
							StringValue = n.Name,
							BackgroundColor = NSColor.Clear,
							DrawsBackground = false,
							Bordered = false,
							Editable = false
						};
						AddSubview (c);
						x += width;
					}
				}
			}
        }

		public override void LoadView ()
		{
			View = materialEditor = new MaterialView {
				ViewModel = ViewModel
			};
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
				Title = "Edit"
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
						var material2 = new ResourceBrushViewController ();
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

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
			var old = View.Frame;
			old.Height = 200;
			View.Frame = old;
        }
    }
}
