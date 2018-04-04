using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using AppKit;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using Xamarin.PropertyEditing.Drawing;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	abstract class ColorEditorView : NSView
	{
		SolidBrushViewModel viewModel;
		protected const float padding = 3;

		public ColorEditorView (IntPtr handle) : base (handle)
		{
		}

		[Export ("initWithCoder:")]
		public ColorEditorView (NSCoder coder) : base (coder)
		{
		}

		public ColorEditorView (CGRect frame) : base (frame)
		{
		}

		public SolidBrushViewModel ViewModel
		{
			get => viewModel;
			set
			{
				if (value == viewModel)
					return;

				if (viewModel != null)
					viewModel.PropertyChanged -= ModelChanged;

				viewModel = value;
				viewModel.PropertyChanged += ModelChanged;
			}
		}

		protected virtual void ModelChanged (object sender, PropertyChangedEventArgs args)
		{
			switch (args.PropertyName) {
				case nameof (SolidBrushViewModel.Color):
					UpdateFromColor (ViewModel.Color);
					break;
			}
		}

		public override void MouseDragged (NSEvent theEvent)
		{
			base.MouseDragged (theEvent);
			UpdateFromEvent (theEvent);
		}

		public override void MouseDown (NSEvent theEvent)
		{
			base.MouseDown (theEvent);
			UpdateFromEvent (theEvent);
		}

		public abstract void UpdateFromColor (CommonColor color);

		public virtual void UpdateFromEvent (NSEvent theEvent)
		{
		}
	}

	abstract class ColorEditorLayer : CALayer
	{
		abstract public void UpdateFromModel (SolidBrushViewModel viewModel);
		abstract public void UpdateFromLocation (SolidBrushViewModel viewModel, CGPoint location);
	}

	class HistoryLayer : ColorEditorLayer
	{
		const float Margin = 3;
		const float BorderRadius = 3;

		public HistoryLayer ()
		{
			var clip = new CALayer () {
				BorderWidth = 1,
				CornerRadius = BorderRadius,
				BorderColor = new CGColor (.5f, .5f, .5f, .5f),
				MasksToBounds = true,
			};
			clip.AddSublayer (Previous);
			clip.AddSublayer (Current);
			AddSublayer (clip);
		}

		CALayer Previous = new CALayer ();
		CALayer Current = new CALayer ();

		public override void LayoutSublayers ()
		{
			base.LayoutSublayers ();
			var clip = Sublayers.First ();

			clip.Frame = new CGRect (
				Margin,
				Margin,
				Frame.Width - 2 * Margin,
				Frame.Height - 2 * Margin);
			
			var width = clip.Frame.Width / 2;

			Previous.Frame = new CGRect (0, 0, width, clip.Frame.Height);
			Current.Frame = new CGRect (width, 0, width, clip.Frame.Height);
		}

		public override void UpdateFromModel (SolidBrushViewModel viewModel)
		{
			LayoutIfNeeded ();
			Current.BackgroundColor = viewModel.Color.ToCGColor ();
			Previous.BackgroundColor = viewModel.LastColor.ToCGColor ();
		}

		public override void UpdateFromLocation (SolidBrushViewModel viewModel, CGPoint location)
		{
		}
	}

	class ShadeLayer : ColorEditorLayer
	{
		const float GripRadius = 3;
		const float BorderRadius = 3;
		const float Margin = 3;

		public ShadeLayer ()
		{
			AddSublayer (Colors);
			Colors.AddSublayer (Black);
			AddSublayer (Grip);
		}

		CALayer Grip = new CALayer {
			BorderColor = new CGColor (.9f, .9f, .9f, .9f),
			BorderWidth = 2,
			CornerRadius = GripRadius,
		};

		CALayer Black = new CAGradientLayer {
			Colors = new[] {
					new CGColor (0f, 0f, 0f, 1f),
					new CGColor (0f, 0f, 0f, 0f)
				},
			CornerRadius = BorderRadius,
		};

		CAGradientLayer Colors = new CAGradientLayer {
			Colors = new[] {
					new CGColor (1f, 1f, 1f),
					new CGColor (1f, .3f, 0f)
				},
			BackgroundColor = NSColor.Black.CGColor,
			BorderColor = new CGColor (.5f, .5f, .5f, .5f),
			BorderWidth = 1,
			CornerRadius = BorderRadius,
		};

        public override void LayoutSublayers ()
        {
            base.LayoutSublayers();

			Colors.Frame = new CGRect (Margin, Margin, Frame.Width - 2 * Margin, Frame.Height - 2 * Margin);
			Black.Frame = new CGRect (0, 0, Frame.Width - 2 * Margin, Frame.Height - 2 * Margin);
			Colors.StartPoint = new CGPoint (0, .5);
			Colors.EndPoint = new CGPoint (1, .5);
        }

		public override void UpdateFromModel (SolidBrushViewModel viewModel)
		{
			LayoutIfNeeded ();
			var color = viewModel.Color;
			var frame = Colors.Frame;
			var x = color.Saturation * frame.Width + frame.X;
			var y = color.Brightness * frame.Height + frame.Y;
			Grip.Frame = new CGRect (x - GripRadius, y - GripRadius, GripRadius * 2, GripRadius * 2);
			float g = (float)(1 - color.Brightness);
			g *= g;
			Grip.BorderColor = new CGColor (g, g, g);
			Colors.Colors = new[] {
				new CGColor (1f, 1f, 1f),
				color.HueColor.ToCGColor ()
			};
		}

		public override void UpdateFromLocation (SolidBrushViewModel viewModel, CGPoint location)
		{
			var loc = location;
			var frame = Colors.Frame;
			loc.X -= frame.X;
			loc.Y -= frame.Y;

			var brightness = loc.Y / frame.Height;
			var saturation = loc.X / frame.Width;

			if (viewModel == null)
				return;

			var color = viewModel.Color;
			viewModel.Color = CommonColor.FromHSB (
				Math.Min (360, Math.Max (0, color.Hue)),
				Math.Min (1, Math.Max (0, saturation)),
				Math.Min (1, Math.Max (0, brightness)));
		}
    }

	class HueLayer : ColorEditorLayer
	{
		const float Margin = 3;
		const float BorderRadius = 3;
		const float GripRadius = 3;

		public HueLayer ()
		{
			AddSublayer (colors);
			AddSublayer (Grip);
		}

		CAGradientLayer colors = new CAGradientLayer {
			BorderColor = new CGColor (.5f, .5f, .5f, .5f),
			BorderWidth = 1,
			CornerRadius = BorderRadius,
			Colors = new[] {
				new CGColor (1,0,0),
				new CGColor (1,0,1),
				new CGColor (0,0,1),
				new CGColor (0,1,1),
				new CGColor (0,1,0),
				new CGColor (1,1,0),
				new CGColor (1,0,0)
			}
		};

		CALayer Grip = new CALayer {
			BorderColor = new CGColor (1f, 1f, 1f),
			BorderWidth = 2,
			CornerRadius = GripRadius,
		};

		public override void UpdateFromModel (SolidBrushViewModel viewModel)
		{
			LayoutIfNeeded ();

			var color = viewModel.Color;
			var hue = color.Hue / 360;
			var pos = colors.Frame.Height * (1 - hue);
			var loc = new CGPoint (1, pos);
			loc.Y -= Grip.Frame.Height / 2f;

			Grip.Frame = new CGRect (loc.X, loc.Y + colors.Frame.Y, Grip.Frame.Width, Grip.Frame.Height);
		}

		public override void UpdateFromLocation (SolidBrushViewModel viewModel, CGPoint location)
		{
			var loc = location;
			var clos = Math.Min (colors.Frame.Height, Math.Max (0, loc.Y - colors.Frame.Y));

			Grip.Frame = new CGRect (1, loc.Y - Grip.Frame.Height / 2, Frame.Width - 2, 2 * GripRadius);
			var hue = (1 - clos/ colors.Frame.Height) * 360;

			if (viewModel == null)
				return;

			var color = viewModel.Color;
			viewModel.Color = CommonColor.FromHSB (
				Math.Max (0, Math.Min (360, hue)),
				Math.Max (0, Math.Min (1, color.Saturation)),
				Math.Max (0, Math.Min (1, color.Brightness)));
		}

        public override void LayoutSublayers()
        {
			base.LayoutSublayers ();
        	colors.Frame = new CGRect (Margin, Margin, Frame.Width - 2 * Margin, Frame.Height - 2 * Margin);
			Grip.Frame = new CGRect (Grip.Frame.X, Grip.Frame.Y, Frame.Width - 2, 2 * GripRadius);
		}
	}

	class ColorComponentEditor : ColorEditorView
	{
		NSTextField Red { get; set; }
		NSTextField Blue { get; set; }
		NSTextField Green { get; set; }
		NSTextField Alpha { get; set; }

		public ColorComponentEditor (CGRect frame) : base (frame)
		{
			Red = new NSTextField (new CGRect (frame.X + padding * 2, frame.Height - 30, 50, 50));
			Green = new NSTextField (new CGRect (frame.X + padding * 2, frame.Height - 80, 50, 50));
			Blue = new NSTextField (new CGRect (frame.X + padding * 2, frame.Height - 120, 50, 50));
			Alpha = new NSTextField (new CGRect (frame.X + padding * 2, frame.Height - 150, 50, 50));
		}

		public override void UpdateFromColor (CommonColor color)
		{
			Red.StringValue = color.R.ToString ();
			Green.StringValue = color.G.ToString ();
			Blue.StringValue = color.B.ToString ();
		}
	}

	public static class DrawingHelpers
	{
		public static CGColor ToCGColor (this CommonColor color)
			=> new CGColor (color.R / 255f, color.G / 255f, color.B / 255f);
	}

	class SolidColorBrushEditor : ColorEditorView
	{
		ShadeLayer Shade = new ShadeLayer ();
		HueLayer Hue = new HueLayer ();
		HistoryLayer History = new HistoryLayer ();

		public override bool AcceptsFirstResponder() => true;
        
        public SolidColorBrushEditor (IntPtr handle) : base (handle)
		{
			InitializeLayers ();
		}

		[Export ("initWithCoder:")]
		public SolidColorBrushEditor (NSCoder coder) : base (coder)
		{
			InitializeLayers ();
		}

		public SolidColorBrushEditor (CGRect frame) : base (frame)
		{
			InitializeLayers ();
		}

		void InitializeLayers ()
		{	
			WantsLayer = true;
			Layer.AddSublayer (Shade);
			Layer.AddSublayer (Hue);
			Layer.AddSublayer (History);
		}

		public override void UpdateFromEvent (NSEvent theEvent)
		{
			var location = ConvertPointToLayer (ConvertPointFromView (theEvent.LocationInWindow, null));

			var hit = Layer.HitTest (new CGPoint (location.X + Layer.Frame.X, location.Y + Layer.Frame.Y));

			for (var c = hit; c != null; c = c.SuperLayer) {
				var editor = c as ColorEditorLayer;
				if (editor != null) {
					editor.UpdateFromLocation (
						ViewModel,
						Layer.ConvertPointToLayer (location, editor));
					break;
				}
			}
		}

		public override void UpdateFromColor (CommonColor color)
		{
			foreach (var editor in Layer.Sublayers.OfType<ColorEditorLayer> ()) {
				editor.UpdateFromModel (ViewModel);
			}
		}

		public override void Layout ()
		{
			base.Layout ();

			var secondarySpan = 20;
			var primarySpan = Frame.Height - 2 * padding - secondarySpan;
			var firstBase = padding;
			var secondBase = padding + secondarySpan;
			var firstStop = firstBase + primarySpan;

			Hue.Frame = new CGRect (firstStop, secondBase, secondarySpan, primarySpan);
			Shade.Frame = new CGRect (firstBase, secondBase, primarySpan, primarySpan);
			History.Frame = new CGRect (firstBase, firstBase, primarySpan, secondarySpan);
			foreach (var editor in Layer.Sublayers.OfType<ColorEditorLayer> ()) {
				editor.UpdateFromModel (ViewModel);
			}
		}
	}
}
