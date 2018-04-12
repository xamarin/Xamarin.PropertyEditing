using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using AppKit;
using CoreAnimation;
using CoreGraphics;
using CoreImage;
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

		public ColorEditorView () : base ()
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
			//base.MouseDragged (theEvent);

			UpdateFromEvent (theEvent);
		}

		public override void MouseDown (NSEvent theEvent)
		{
			//base.MouseDown (theEvent);
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

	class CommonGradientBrushLayer : CALayer
	{
		CommonGradientBrush brush;
		public CommonGradientBrush Brush
		{
			get => brush;
			set {
				brush = value;
				SetNeedsDisplay ();
			}
		}

		public override void DrawInContext (CGContext ctx)
		{
			ctx.SaveState ();

			var colorspace = CGColorSpace.CreateDeviceRGB ();
			var colors = Brush.GradientStops.Select (stop => stop.Color.ToCGColor ()).ToArray ();
			var locations = Brush.GradientStops.Select (stop => (nfloat)stop.Offset).ToArray ();

			var gradient = new CGGradient (colorspace, colors, locations);
			var center = new CGPoint (Bounds.Width / 2f, Bounds.Height / 2f);
			var radius = (float)Math.Min (Bounds.Width / 2.0, Bounds.Height / 2.0);

			switch (Brush) {
				case CommonLinearGradientBrush linear:
					ctx.DrawLinearGradient (gradient, new CGPoint (0, 0), new CGPoint (0, Bounds.Width), CGGradientDrawingOptions.None);
					break;
				case CommonRadialGradientBrush radial:
					ctx.DrawRadialGradient (gradient, startCenter: center, startRadius: 0f, endCenter: center, endRadius: radius, options: CGGradientDrawingOptions.None);
					break;
			}
        }
    }

	class CommonBrushLayer : CALayer
	{
		public CommonBrushLayer ()
		{
			this.CornerRadius = 3;
			this.BorderColor = new CGColor (.5f, .5f, .5f, .5f);
			this.BorderWidth = 1;
			MasksToBounds = true;
		}

		CALayer brushLayer;
		CALayer BrushLayer {
			get => brushLayer;
			set {
				if (brushLayer != null)
					brushLayer.RemoveFromSuperLayer ();

				brushLayer = value;
		
				if (brushLayer != null)
					AddSublayer (brushLayer);
			}
		}

		CommonBrush brush;
		public CommonBrush Brush {
			get => brush;
			set {
				brush = value;
				BrushLayer = CreateBrushLayer (brush);
				Opacity = brush == null ? 0 : 1;
			}
		}

		public static CALayer CreateBrushLayer (CommonBrush brush)
		{
			switch (brush) {
				case CommonSolidBrush solid:
					return new CALayer {
						BackgroundColor = solid.Color.ToCGColor (),
						Opacity = (float)solid.Opacity
					};
				case CommonGradientBrush gradient:
					return new CommonGradientBrushLayer {
						Opacity = (float)gradient.Opacity
					};
				default:
					return new CALayer {
						BackgroundColor = NSColor.Clear.CGColor
					};
			}
		}

		public override void LayoutSublayers ()
		{
			base.LayoutSublayers ();
			BrushLayer.Frame = new CGRect (0, 0, Frame.Width, Frame.Height);
			Contents = DrawingExtensions.GenerateCheckerboard (Frame);
		}

		public NSImage RenderPreview ()
		{
			var scale = this.ContentsScale;
			nint h = (nint)(this.Bounds.Height * scale);
			nint w = (nint)(this.Bounds.Width * scale);
			nint bytesPerRow = w * 4;

			if (h <= 0 || w <= 0)
				return null;

			using (var colorSpace = CGColorSpace.CreateGenericRgb ())
			using (var context = new CGBitmapContext (IntPtr.Zero, w, h, 8, bytesPerRow, colorSpace, CGImageAlphaInfo.PremultipliedLast)) {
				this.RenderInContext (context);
				using (var image = context.ToImage ()) {
					return new NSImage (image, new CGSize (w, h));
				}
			}
		}
	}

	class HistoryLayer : ColorEditorLayer
	{
		const float Margin = 3;
		const float BorderRadius = 3;

		public HistoryLayer ()
		{
			Clip.AddSublayer (Previous);
			Clip.AddSublayer (Current);
			AddSublayer (Clip);
		}

		readonly CALayer Previous = new CALayer ();
		readonly CALayer Current = new CALayer ();
		readonly CALayer Clip = new CALayer () {
			BorderWidth = 1,
			CornerRadius = BorderRadius,
			BorderColor = new CGColor (.5f, .5f, .5f, .5f),
			MasksToBounds = true,
		};

		public override void LayoutSublayers ()
		{
			base.LayoutSublayers ();

			Clip.Frame = new CGRect (
				Margin,
				Margin,
				Frame.Width - 2 * Margin,
				Frame.Height - 2 * Margin);
			
			Clip.Contents = DrawingExtensions.GenerateCheckerboard (Clip.Frame);
			var width = Clip.Frame.Width / 2;

			Previous.Frame = new CGRect (0, 0, width, Clip.Frame.Height);
			Current.Frame = new CGRect (width, 0, width, Clip.Frame.Height);
		}

		public override void UpdateFromModel (SolidBrushViewModel viewModel)
		{
			LayoutIfNeeded ();
			Current.BackgroundColor = viewModel.Color.ToCGColor ();
			Previous.BackgroundColor = viewModel.LastColor.ToCGColor ();
		}

		public override void UpdateFromLocation (SolidBrushViewModel viewModel, CGPoint location)
		{
			if (Previous == HitTest (location))
				viewModel.Color = viewModel.LastColor;
		}
	}

	class ShadeLayer : ColorEditorLayer
	{
		const float GripRadius = 4;
		const float BorderRadius = 3;
		const float Margin = 3;

		public ShadeLayer ()
		{
			AddSublayer (Colors);
			Colors.AddSublayer (Black);
			AddSublayer (Grip);
			float innerRadius = GripRadius - 1;

			Grip.AddSublayer (new CALayer {
				BorderWidth = 1,
				BorderColor = new CGColor (0, 0, 0),
				Frame = new CGRect (
					GripRadius - innerRadius,
					GripRadius - innerRadius,
					innerRadius * 2,
					innerRadius * 2),
				CornerRadius = innerRadius
			});
		}

		CALayer Grip = new CALayer {
			BorderColor = new CGColor (1,1,1),
			BorderWidth = 1,
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
				Math.Min (1, Math.Max (0, brightness)),
				color.A);
		}
    }

	class HueLayer : ColorEditorLayer
	{
		const float Margin = 3;
		const float BorderRadius = 3;
		const float GripRadius = 3;

		public CGColor GripColor
		{
			get => Grip.BorderColor;
			set => Grip.BorderColor = value;
		}

		public HueLayer ()
		{
			AddSublayer (Colors);
			AddSublayer (Grip);
		}

		CAGradientLayer Colors = new CAGradientLayer {
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
			BorderColor = NSColor.Text.CGColor,
			BorderWidth = 2,
			CornerRadius = GripRadius,
		};

		public override void UpdateFromModel (SolidBrushViewModel viewModel)
		{
			LayoutIfNeeded ();

			var color = viewModel.Color;
			var hue = color.Hue / 360;
			var pos = Colors.Frame.Height * (1 - hue);
			var loc = new CGPoint (1, pos);
			loc.Y -= Grip.Frame.Height / 2f;

			Grip.Frame = new CGRect (loc.X, loc.Y + Colors.Frame.Y, Grip.Frame.Width, Grip.Frame.Height);
		}

		public override void UpdateFromLocation (SolidBrushViewModel viewModel, CGPoint location)
		{
			var loc = location;
			var clos = Math.Min (Colors.Frame.Height, Math.Max (0, loc.Y - Colors.Frame.Y));

			Grip.Frame = new CGRect (1, loc.Y - Grip.Frame.Height / 2, Frame.Width - 2, 2 * GripRadius);
			var hue = (1 - clos/ Colors.Frame.Height) * 360;

			if (viewModel == null)
				return;

			var color = viewModel.Color;
			viewModel.Color = CommonColor.FromHSB (
				Math.Max (0, Math.Min (360, hue)),
				Math.Max (0, Math.Min (1, color.Saturation)),
				Math.Max (0, Math.Min (1, color.Brightness)),
				color.A);
		}

        public override void LayoutSublayers()
        {
			base.LayoutSublayers ();
			Colors.Frame = Frame.Bounds ().Border (new CommonThickness (2));
        	//Colors.Frame = new CGRect (Margin, Margin, Frame.Width - 2 * Margin, Frame.Height - 2 * Margin);
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

	class SolidColorBrushEditor : ColorEditorView
	{
		ShadeLayer Shade = new ShadeLayer ();
		HueLayer Hue = new HueLayer ();
		HistoryLayer History = new HistoryLayer ();
		CALayer Background = new CALayer {
			CornerRadius = 3,
			BorderWidth = 1
		};

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

		public SolidColorBrushEditor () : base ()
		{
			InitializeLayers ();	
		}

		void InitializeLayers ()
		{
			Layer = new CALayer ();
			Layer.AddSublayer (Background);
			Layer.AddSublayer (Shade);
			Layer.AddSublayer (Hue);
			Layer.AddSublayer (History);
			WantsLayer = true;
		}

		public override void UpdateFromEvent (NSEvent theEvent)
		{
			var location = ConvertPointFromView (theEvent.LocationInWindow, null);
			location = ConvertPointToLayer (location);

			foreach (var layer in Layer.Sublayers) {
				var hit = layer.HitTest (location);

				for (var c = hit; c != null; c = c.SuperLayer) {
					var editor = c as ColorEditorLayer;
					if (editor != null) {
						editor.UpdateFromLocation (
							ViewModel,
							Layer.ConvertPointToLayer (location, editor));
						return;
					}
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

			Background.BorderColor = new CGColor (.5f, .5f, .5f, .5f);
			Background.BackgroundColor = NSColor.ControlBackground.CGColor;
			Background.Frame = new CGRect (0, 0, Frame.Height, Frame.Height);
			Hue.Frame = new CGRect (firstStop, secondBase, secondarySpan, primarySpan);
			Hue.GripColor = NSColor.Text.CGColor;
			Shade.Frame = new CGRect (firstBase, secondBase, primarySpan, primarySpan);
			History.Frame = new CGRect (firstBase, firstBase, primarySpan, secondarySpan);
			foreach (var editor in Layer.Sublayers.OfType<ColorEditorLayer> ()) {
				editor.UpdateFromModel (ViewModel);
			}
		}
	}
}
