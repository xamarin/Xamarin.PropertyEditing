using System;
using System.Linq;
using AppKit;
using CoreAnimation;
using CoreGraphics;
using Xamarin.PropertyEditing.Drawing;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class MaterialView : NSView
	{
		public override bool IsFlipped => true;

		public MaterialView ()
		{
			Initialize ();
		}

		private void Initialize ()
		{
			WantsLayer = true;
		}

		private BrushPropertyViewModel viewModel;
		public BrushPropertyViewModel ViewModel {
			get => viewModel;
			set {
				if (viewModel == value)
					return;

				viewModel = value;
				NeedsLayout = true;
			}
		}

		public MaterialDesignColorViewModel MaterialDesign {
			get => ViewModel?.MaterialDesign;
		}

		public override void Layout ()
		{
			if (Layer?.Sublayers != null) {
				foreach (var l in Layer.Sublayers) {
					l.RemoveFromSuperLayer ();
					l.Dispose ();
				}
			}

			if (MaterialDesign == null)
				return;

			var colors = MaterialDesign.Palettes.Select (p => new { p.Name, Color = p.MainColor }).ToArray ();
			int col = 0;
			nfloat x = 0;
			nfloat y = 6;
			var width = (Frame.Width - 54) / 10;
			var height = (Frame.Height - 49) / 4;

			MaterialColorLayer CreateLayer (CommonColor color)
			{
				var selectedColor = color.Lightness > 0.58 ? NSColor.Black : NSColor.White;
				return new MaterialColorLayer {
					BackgroundColor = color,
					ForegroundColor = selectedColor.CGColor,
					Text = color.Label,
					FontSize = 12,
					ContentsScale = NSScreen.MainScreen.BackingScaleFactor,
					TextAlignmentMode = CATextLayerAlignmentMode.Center,
				};
			}

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
					IsSelected = MaterialDesign.Color == p.Color || MaterialDesign.ColorName == p.Name
				};

				l.BorderColor = new CGColor (.5f, .5f, .5f, .5f);
				l.Frame = new CGRect (x, y, width, height);
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
			var normal = new CALayer {
				CornerRadius = 3,
				MasksToBounds = true,
				Frame = new CGRect (x, y, Frame.Width, height),
				BorderColor = new CGColor (.5f, .5f, .5f, .5f),
				BorderWidth = 1
			};
			Layer.AddSublayer (normal);
			foreach (var color in MaterialDesign.NormalColorScale) {
				var l = CreateLayer (color.Value);
				l.ColorType = MaterialColorType.Normal;
				l.IsSelected = color.Value == MaterialDesign.NormalColor || color.Value == MaterialDesign.Color || color.Value == ViewModel.Solid.Color;
				l.Frame = new CGRect (x, 0, width, height);
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
			foreach (var color in MaterialDesign.AccentColorScale) {
				var l = CreateLayer (color.Value);
				l.ColorType = MaterialColorType.Accent;
				l.IsSelected = color.Value == MaterialDesign.AccentColor || color.Value == MaterialDesign.Color || color.Value == ViewModel.Solid.Color;
				l.Frame = new CGRect (x, 0, width, height);
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
						switch (editor.ColorType) {
						case MaterialColorType.Accent:
							MaterialDesign.AccentColor = editor.BackgroundColor;
							break;
						default:
							MaterialDesign.NormalColor = editor.BackgroundColor;
							break;
						}
						NeedsLayout = true;
					}
				}
			}
		}
	}
}