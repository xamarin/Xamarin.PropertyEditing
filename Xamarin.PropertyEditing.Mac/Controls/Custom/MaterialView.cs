using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using AppKit;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using Xamarin.PropertyEditing.Drawing;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class MaterialView : NotifyingView<MaterialDesignColorViewModel>
	{
		public override bool AcceptsFirstResponder ()
		{
			return false;
		}

		public override void OnViewModelChanged (MaterialDesignColorViewModel oldModel)
		{
			base.OnViewModelChanged (oldModel);

			CreateColourPallette ();
		}

		private void CreateColourPallette ()
		{
			var subViews = Subviews;
			if (subViews.Length > 0) {
				foreach (var sv in subViews) {
					if (sv is FocusableButton fb)
						fb.Activated -= MaterialColourButton_Activated;
					sv.RemoveFromSuperview ();
					sv.Dispose ();
				}
			}

			if (ViewModel == null)
				return;

			var colors = ViewModel.Palettes.Select (p => new { p.Name, Color = p.MainColor }).ToArray ();
			int col = 0;
			nfloat x = 0;
			nfloat y = 6;
			const int FrameWidth = 430; // TODO Get proper Frame.Width, but hacking to get this working
			const int FrameHeight = 202; // TODO Get proper Frame.Height, but hacking to get this working
			var width = (FrameWidth - 54) / 10;
			var height = (FrameHeight - 49) / 4;


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
					Frame = new CGRect (0, 0, width, height),
					ForegroundColor = selectedColor.CGColor,
					BackgroundColor = p.Color,
					CornerRadius = 3,
					BorderColor = new CGColor (.5f, .5f, .5f, .5f),
					MasksToBounds = false,
					IsSelected = ViewModel.Color == p.Color || ViewModel.ColorName == p.Name
				};

				var materialColourButton = new FocusableButton {
					Frame = frame,
					WantsLayer = true,
					Layer = l,
					ToolTip = p.Name,
					Transparent = false,
					TranslatesAutoresizingMaskIntoConstraints = true,
				};

				materialColourButton.Activated += MaterialColourButton_Activated;

				AddSubview (materialColourButton);

				x += width + 6;
				col++;
				if (col >= 10) {
					x = 0;
					y += height + 6;
					col = 0;
				}
			}

			var colourName = new UnfocusableTextField {
				Frame = new CGRect (x, y + 6, FrameWidth, PropertyEditorControl.DefaultControlHeight),
				StringValue = ViewModel.ColorName,
				TranslatesAutoresizingMaskIntoConstraints = true,
			};

			AddSubview (colourName);

			y += 25;
			x = 0;
			width = FrameWidth / ViewModel.NormalColorScale.Count ();

			foreach (var color in ViewModel.NormalColorScale) {
				var l = CreateLayer (color.Value);
				l.ColorType = MaterialColorType.Normal;
				l.IsSelected = color.Value == ViewModel.NormalColor || color.Value == ViewModel.Color;
				l.Frame = new CGRect (0, 0, width, height);

				var normalColourButton = new FocusableButton {
					Frame = new CGRect (x, y, width, height),
					WantsLayer = true,
					Layer = l,
					ToolTip = color.ToString (),
					Transparent = false,
					TranslatesAutoresizingMaskIntoConstraints = true,
				};

				normalColourButton.Activated += MaterialColourButton_Activated;

				AddSubview (normalColourButton);

				x += width;
			}

			if (ViewModel.AccentColorScale.Count () <= 0) {
				Window?.RecalculateKeyViewLoop (); // Still needs to be called for the Buttons above.
				return;
			}

			y += height + 6;
			x = 0;

			width = FrameWidth / ViewModel.AccentColorScale.Count ();
			foreach (var color in ViewModel.AccentColorScale) {
				var l = CreateLayer (color.Value);
				l.ColorType = MaterialColorType.Accent;
				l.IsSelected = color.Value == ViewModel.AccentColor || color.Value == ViewModel.Color;
				l.Frame = new CGRect (0, 0, width, height);

				var accentColourButton = new FocusableButton {
					Frame = new CGRect (x, y, width, height),
					WantsLayer = true,
					Layer = l,
					ToolTip = color.ToString (),
					Transparent = false,
					TranslatesAutoresizingMaskIntoConstraints = true,
				};

				accentColourButton.Activated += MaterialColourButton_Activated;

				AddSubview (accentColourButton);

				x += width;
			}

			Window?.RecalculateKeyViewLoop ();
		}

		private void MaterialColourButton_Activated (object sender, EventArgs e)
		{
			var button = sender as FocusableButton;
			var editor = button?.Layer as MaterialColorLayer;

			if (editor != null) {
				switch (editor.ColorType) {
					case MaterialColorType.Accent:
						ViewModel.AccentColor = editor.BackgroundColor;
						break;
					case MaterialColorType.Normal:
						ViewModel.NormalColor = editor.BackgroundColor;
						break;
					case MaterialColorType.Palette:
						var match = ViewModel.Palettes.First (palette => palette.MainColor == editor.BackgroundColor);
						ViewModel.ColorName = match.Name;
						break;
				}
			}
		}

		public override void OnPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName) {
				case nameof (ViewModel.ColorName):
				case nameof (ViewModel.AccentColor):
				case nameof (ViewModel.NormalColor):
					CreateColourPallette ();
					break;
			}
		}
	}
}
