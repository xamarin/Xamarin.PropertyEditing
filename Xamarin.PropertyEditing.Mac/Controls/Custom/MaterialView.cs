using System;
using System.Linq;
using AppKit;
using CoreAnimation;
using CoreGraphics;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	class MaterialView : NSView  
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

		void Initialize ()
		{
			WantsLayer = true;
		}

		BrushPropertyViewModel viewModel;
		public BrushPropertyViewModel ViewModel
		{
			get => viewModel;
			set
			{
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
}