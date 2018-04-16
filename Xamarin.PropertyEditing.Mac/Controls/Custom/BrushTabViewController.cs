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

		public override void OnPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName) {
				case nameof (BrushPropertyViewModel.MaterialDesign):
					materialEditor.ViewModel = ViewModel;
					break;
			}
		}

		public override void OnViewModelChanged (BrushPropertyViewModel oldModel)
		{
			if (materialEditor != null)
				materialEditor.ViewModel = ViewModel;
		}

		public class MaterialColorLayer : CATextLayer
		{
			public MaterialColorLayer ()
			{
				AddSublayer (Selection);
			}

			CATextLayer Selection { get; } = new CATextLayer () {
				CornerRadius = 3
			};

			string text;
			public string Text {
				get => text;
				set {
					text = value;
					SetNeedsLayout ();
				}
			}

			CommonColor materialColor;
			public new CommonColor BackgroundColor
			{
				get => materialColor;
				set
				{
					materialColor = value;
					base.BackgroundColor = materialColor.ToCGColor ();
				}
			}

			bool isSelected;
			public bool IsSelected {
				get => isSelected;
				set {
					if (isSelected == value)
						return;
					isSelected = value;
					SetNeedsLayout ();
				}
			}

			public override void LayoutSublayers ()
			{
				base.LayoutSublayers ();
				//String = isSelected ? "" : text;
				Selection.String = text;
				Selection.Frame = Frame.Bounds ().Border (new CommonThickness (3));
				Selection.BorderWidth = isSelected ? 2 : 0;
				Selection.BorderColor = ForegroundColor;
				Selection.ForegroundColor = ForegroundColor;
				Selection.FontSize = FontSize;
				Selection.ContentsScale = ContentsScale;
				Selection.TextAlignmentMode = TextAlignmentMode;
			}
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

			readonly string [] AccentNames = {
				"A100",
				"A200",
				"A400",
				"A700"
			};

			readonly string [] BlackWhite = {
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

			public MaterialDesignColorViewModel MaterialDesign
			{
				get => ViewModel?.MaterialDesign;
			}

			public override void Layout ()
			{
				if (Layer?.Sublayers != null)
					foreach (var l in Layer.Sublayers)
						l.RemoveFromSuperLayer ();

				if (MaterialDesign == null)
					return;
				
				var colors = MaterialDesign.Palettes.Select (p => new { p.Name, Color = p.MainColor }).ToArray ();
				int col = 0;
				nfloat x = 0;
				nfloat y = 6;
				var width = (Frame.Width - 54) / 10;
				var height = (Frame.Height - 49) / 4;

				foreach (var p in colors) {
					var frame = new CGRect (x, y, width, height);
					var selectedColor = p.Color.Lightness > 0.58 ? NSColor.Black : NSColor.White;
					var l = new MaterialColorLayer {
						Frame = frame,
						ForegroundColor = selectedColor.CGColor,
						BackgroundColor = p.Color,
						CornerRadius = 3,
						BorderColor = new CGColor (.5f, .5f, .5f, .5f),
						MasksToBounds = false,
						IsSelected = MaterialDesign.Color == p.Color
					};

					Layer.AddSublayer (l);
					x += width + 6;
					col++;
					if (col >= 10) {
						x = 0;
						y += height + 6;
						col = 0;
					}
				}

				Layer.AddSublayer (new CATextLayer {
					ForegroundColor = NSColor.ControlText.CGColor,
					Frame = new CGRect (x, y + 6, Frame.Width, 25),
					String = MaterialDesign.ColorName,
					FontSize = NSFont.SmallSystemFontSize,
					ContentsScale = Window?.Screen?.BackingScaleFactor ?? NSScreen.MainScreen.BackingScaleFactor
				});

				y += 25;
				x = 0;
				width = Frame.Width / MaterialDesign.NormalColorScale.Count ();
				var names = MaterialDesign.NormalColorScale.Count () > 2 ? ColorNames : BlackWhite;
				var normal = new CALayer {
					CornerRadius = 3,
					MasksToBounds = true,
					Frame = new CGRect (x, y, Frame.Width, height),
					BorderColor = new CGColor (.5f, .5f, .5f, .5f),
					BorderWidth = 1
				};

				Layer.AddSublayer (normal);
				foreach (var n in MaterialDesign.NormalColorScale.Zip (names, (c, name) => new { Name = name, Color = c.Value })) {
					var frame = new CGRect (x, y, width, height);
					var selectedColor = n.Color.Lightness > 0.58 ? NSColor.Black : NSColor.White;
					var l = new MaterialColorLayer {
						BackgroundColor = n.Color,
						ForegroundColor = selectedColor.CGColor,
						Frame = new CGRect (x, 0, width, height),
						Text = n.Name,
						FontSize = 12,
						ContentsScale = NSScreen.MainScreen.BackingScaleFactor,
						TextAlignmentMode = CATextLayerAlignmentMode.Center,
						IsSelected = MaterialDesign.Color == n.Color
					};
					normal.AddSublayer (l);
					x += width;
				}

				if (MaterialDesign.AccentColorScale.Count () <= 0)
					return;

				y += height + 6;
				x = 0;

				var accent = new CALayer {
					CornerRadius = 3,
					MasksToBounds = true,
					Frame = new CGRect (x, y, Frame.Width, height),
					BorderColor = new CGColor (.5f, .5f, .5f, .5f),
					BorderWidth = 1
				};
				Layer.AddSublayer (accent);
				width = Frame.Width / MaterialDesign.AccentColorScale.Count ();
				foreach (var n in MaterialDesign.AccentColorScale.Zip (AccentNames, (c, n) => new { Name = n, Color = c.Value })) {
					var frame = new CGRect (x, y, width, height);
					var selectedColor = n.Color.Lightness > 0.58 ? NSColor.Black : NSColor.White;
					var l = new MaterialColorLayer {
						BackgroundColor = n.Color,
						ForegroundColor = selectedColor.CGColor,
						Frame = new CGRect (x, 0, width, height),
						Text = n.Name,
						FontSize = 12,
						ContentsScale = NSScreen.MainScreen.BackingScaleFactor,
						TextAlignmentMode = CATextLayerAlignmentMode.Center,
						IsSelected = ViewModel.Solid.Color == n.Color,
					};

					accent.AddSublayer (l);
					x += width;
				}
			}

			public override void MouseDown (NSEvent theEvent)
			{
				UpdateFromEvent (theEvent);
			}

			public void UpdateFromEvent (NSEvent theEvent)
			{
				var location = ConvertPointToLayer (ConvertPointFromView (theEvent.LocationInWindow, null));

				foreach (var layer in Layer.Sublayers) {
					var hit = layer.HitTest (location);
					for (var c = hit; c != null; c = c.SuperLayer) {
						var editor = c as MaterialColorLayer;
						if (editor != null) {
							ViewModel.Solid.Color = editor.BackgroundColor;
							NeedsLayout = true;
						}
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

	class EmptyBrushEditorViewController : PropertyViewController<BrushPropertyViewModel>
	{
		NSButton brushEditor;

		public EmptyBrushEditorViewController ()
		{
			PreferredContentSize = new CGSize (100, 100);
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

		public override void OnPropertyChanged (object sender, PropertyChangedEventArgs args)
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
            base.ViewDidLoad();
			//View.Appearance = PropertyEditorPanel.ThemeManager.CurrentAppearance;
			var old = View.Frame;
			old.Height = 200;
			View.Frame = old;
        }
    }
}
