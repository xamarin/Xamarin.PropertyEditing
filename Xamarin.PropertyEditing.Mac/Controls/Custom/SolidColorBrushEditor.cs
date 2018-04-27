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
	abstract class ColorEditorView : NSView, INotifyingListner<SolidBrushViewModel>
	{
		protected const float padding = 3;
		protected NotifyingViewAdaptor<SolidBrushViewModel> adaptor { get; }

		public ColorEditorView (IntPtr handle) : base (handle)
		{
			adaptor = new NotifyingViewAdaptor<SolidBrushViewModel> (this);
		}

		[Export ("initWithCoder:")]
		public ColorEditorView (NSCoder coder) : base (coder)
		{
			adaptor = new NotifyingViewAdaptor<SolidBrushViewModel> (this);
		}

		public ColorEditorView (CGRect frame) : base (frame)
		{
			adaptor = new NotifyingViewAdaptor<SolidBrushViewModel> (this);
		}

		public ColorEditorView () : base ()
		{
			adaptor = new NotifyingViewAdaptor<SolidBrushViewModel> (this);
		}

		public SolidBrushViewModel ViewModel {
			get => adaptor.ViewModel;
			set => adaptor.ViewModel = value;
		}

		protected virtual void OnViewModelChanged (SolidBrushViewModel oldModel)
		{
			OnPropertyChanged (ViewModel, new PropertyChangedEventArgs (nameof (SolidBrushViewModel.Color)));
		}

		protected virtual void OnPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
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

		public virtual void UpdateFromEvent (NSEvent theEvent)
		{
		}

		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);

			if (!disposing)
				return;

			adaptor.Disconnect ();
		}

		void INotifyingListner<SolidBrushViewModel>.OnViewModelChanged (SolidBrushViewModel oldModel)
		{
			OnViewModelChanged (oldModel);
		}

		void INotifyingListner<SolidBrushViewModel>.OnPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			OnPropertyChanged (sender, e);
		}
	}

	abstract class ColorEditorLayer : CALayer
	{
		public ColorEditorLayer ()
		{
		}

		public ColorEditorLayer (IntPtr handle) : base (handle)
		{
		}

		abstract public void UpdateFromModel (EditorInteraction viewModel);
		abstract public void UpdateFromLocation (EditorInteraction viewModel, CGPoint location);
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

		public HistoryLayer (IntPtr handle) : base (handle)
		{
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

		public override void UpdateFromModel (EditorInteraction interaction)
		{
			LayoutIfNeeded ();
			Current.BackgroundColor = interaction.Color.ToCGColor ();
			Previous.BackgroundColor = interaction.LastColor.ToCGColor ();
		}

		public override void UpdateFromLocation (EditorInteraction interaction, CGPoint location)
		{
			if (Previous == HitTest (location))
				interaction.Color = interaction.LastColor;
		}
	}

	class ShadeLayer : ColorEditorLayer
	{
		const float GripRadius = 4;
		const float BorderRadius = 3;
		const float Margin = 3;
		ChannelEditor saturationEditor = new HsbSaturationChannelEditor ();
		ChannelEditor brightnessEditor = new HsbBrightnessChannelEditor ();

		public ShadeLayer ()
		{
			AddSublayer (Saturation);
			Saturation.AddSublayer (Brightness);
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

		public ShadeLayer (IntPtr handle) : base (handle)
		{
		}

		CALayer Grip = new CALayer {
			BorderColor = new CGColor (1,1,1),
			BorderWidth = 1,
			CornerRadius = GripRadius,
		};

		CAGradientLayer Brightness = new CAGradientLayer {
			Colors = new[] {
					new CGColor (0f, 0f, 0f, 1f),
					new CGColor (0f, 0f, 0f, 0f)
				},
			CornerRadius = BorderRadius,
		};

		CAGradientLayer Saturation = new CAGradientLayer {
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

			Saturation.Frame = new CGRect (Margin, Margin, Frame.Width - 2 * Margin, Frame.Height - 2 * Margin);
			Brightness.Frame = Saturation.Frame.Bounds ();
			Saturation.StartPoint = new CGPoint (0, .5);
			Saturation.EndPoint = new CGPoint (1, .5);
        }

		CommonColor c;
		public override void UpdateFromModel (EditorInteraction interaction)
		{
			LayoutIfNeeded ();
			var color = interaction.Color;

			var sat = saturationEditor.LocationFromColor (Saturation, color);
			var bright = brightnessEditor.LocationFromColor (Brightness, color);

			var x = sat.X;
			var y = bright.Y + Saturation.Frame.Y;

			Grip.Frame = new CGRect (x - GripRadius, y - GripRadius, GripRadius * 2, GripRadius * 2);

			if (interaction.StartColor.ToCGColor () != Saturation.Colors.Last())
				saturationEditor.UpdateGradientLayer (Saturation, interaction.StartColor.HueColor);
		}

		public override void UpdateFromLocation (EditorInteraction interaction, CGPoint location)
		{
			var loc = location;
			var frame = Saturation.Frame;

			if (interaction.ViewModel == null)
				return;

			var color = interaction.Color;
			var saturation = saturationEditor.ValueFromLocation (Saturation, loc);
			var brightness = saturationEditor.ValueFromLocation (
				Brightness, 
			    new CGPoint (loc.X + Brightness.Frame.X, loc.Y + Brightness.Frame.Y));
			
			c = interaction.Color = interaction.Color.UpdateHSB (saturation: saturation, brightness: brightness);
		}
    }

	class HueLayer : ColorEditorLayer
	{
		const float Margin = 3;
		const float BorderRadius = 3;
		const float GripRadius = 3;
		ChannelEditor hueEditor = new HsbHueChannelEditor ();

		public CGColor GripColor
		{
			get => Grip.BorderColor;
			set => Grip.BorderColor = value;
		}

		public HueLayer ()
		{
			hueEditor.UpdateGradientLayer (Colors, new CommonColor (0,255,0));
			AddSublayer (Colors);
			AddSublayer (Grip);
		}

		public HueLayer (IntPtr handle) : base (handle)
		{
		}

		CAGradientLayer Colors = new CAGradientLayer {
			BorderColor = new CGColor (.5f, .5f, .5f, .5f),
			StartPoint = new CGPoint (0,1),
			EndPoint = new CGPoint (0,0),
			BorderWidth = 1,
			CornerRadius = BorderRadius,
		};

		CALayer Grip = new CALayer {
			BorderColor = NSColor.Text.CGColor,
			BorderWidth = 2,
			CornerRadius = GripRadius,
		};

		CommonColor c;
		public override void UpdateFromModel (EditorInteraction interaction)
		{
			LayoutIfNeeded ();

			var color = interaction.Color;
			if (c == color)
				return;

			var loc = hueEditor.LocationFromColor (Colors, color);
			Grip.Frame = new CGRect (1, loc.Y - Grip.Frame.Height / 2f, Grip.Frame.Width, Grip.Frame.Height);
		}

		public override void UpdateFromLocation (EditorInteraction interaction, CGPoint location)
		{
			var loc = location;
			var clos = Math.Min (Colors.Frame.Height, Math.Max (0, loc.Y - Colors.Frame.Y));

			Grip.Frame = new CGRect (1, clos + Colors.Frame.Y - Grip.Frame.Height / 2f, Frame.Width - 2, 2 * GripRadius);
			var hue = (1 - clos/ Colors.Frame.Height) * 360;

			if (interaction == null)
				return;

			var color = interaction.Color;
			c = interaction.Color = hueEditor.UpdateColorFromLocation (
				Colors,
				interaction.Color,
				loc);
		}

        public override void LayoutSublayers()
        {
			base.LayoutSublayers ();
			Colors.Frame = Frame.Bounds ().Border (new CommonThickness (2));
			Grip.Frame = new CGRect (Grip.Frame.X, Grip.Frame.Y, Frame.Width - 2, 2 * GripRadius);
		}
	}

	class EditorInteraction : NotifyingObject
	{
		public EditorInteraction (SolidBrushViewModel viewModel, ColorEditorLayer layer)
		{
			ViewModel = viewModel;
			StartColor = Color = viewModel.Color;
			Layer = layer;
		}

		public ColorEditorLayer Layer { get; set; }
		public SolidBrushViewModel ViewModel { get; set; }

		public CommonColor StartColor { get; }

		CommonColor color;
		public CommonColor Color
		{
			get => color;
			set {
				if (color == value)
					return;

				color = value;
				OnPropertyChanged ();
			}
		}

		public CommonColor LastColor {
			get => ViewModel.LastColor;
		}

		public void Commit ()
		{
			ViewModel.Color = Color;
		}
	}

	class SolidColorBrushEditor : ColorEditorView
	{
		ShadeLayer Shade = new ShadeLayer ();
		HueLayer Hue = new HueLayer ();
		HistoryLayer History = new HistoryLayer ();

		readonly CALayer Background = new CALayer {
			CornerRadius = 3,
			BorderWidth = 1
		};

		readonly CALayer componentBackground = new CALayer {
			CornerRadius = 3,
			BorderWidth = 1
		};

		readonly ColorComponentTabViewController componentTabs = new ColorComponentTabViewController () {
			EditorType = ChannelEditorType.RGB
		};

		public override bool AcceptsFirstResponder() => true;
        
        public SolidColorBrushEditor (IntPtr handle) : base (handle)
		{
			Initialize ();
		}

		[Export ("initWithCoder:")]
		public SolidColorBrushEditor (NSCoder coder) : base (coder)
		{
			Initialize ();
		}

		public SolidColorBrushEditor (CGRect frame) : base (frame)
		{
			Initialize ();
		}

		public SolidColorBrushEditor () : base ()
		{
			Initialize ();	
		}

		void Initialize ()
		{
			Layer = new CALayer ();
			Layer.AddSublayer (Background);
			Layer.AddSublayer (Shade);
			Layer.AddSublayer (Hue);
			Layer.AddSublayer (History);
			Layer.AddSublayer (componentBackground);
			WantsLayer = true;
			AddSubview (componentTabs.View);
		}

		EditorInteraction interaction;
		public override void UpdateFromEvent (NSEvent theEvent)
		{
		}

		public override void MouseDown (NSEvent theEvent)
		{
			var location = ConvertPointFromView (theEvent.LocationInWindow, null);
			location = ConvertPointToLayer (location);
			interaction = null;
			foreach (var layer in Layer.Sublayers) {
				var hit = layer.PresentationLayer.HitTest (location);

				for (var c = hit?.ModelLayer; c != null; c = c.SuperLayer) {
					var active = c as ColorEditorLayer;
					if (active != null) {
						interaction = new EditorInteraction (ViewModel, active);
						active.UpdateFromLocation (
							interaction,
							Layer.ConvertPointToLayer (location, active));
						OnPropertyChanged (ViewModel, new PropertyChangedEventArgs (nameof (SolidBrushViewModel.Color)));
						return;
					}
				}
			}
		}

		CGPoint last;
        public override void MouseDragged(NSEvent theEvent)
        {
			var location = ConvertPointFromView (theEvent.LocationInWindow, null);
			var diff = new CGPoint (last.X - location.X, last.Y - location.Y);

			if (diff.X * diff.X < .5 && diff.Y * diff.Y < .5)
				return;

			interaction?.Layer?.UpdateFromLocation (
						interaction,
						Layer.ConvertPointToLayer (location, interaction.Layer));

			OnPropertyChanged (ViewModel, new PropertyChangedEventArgs (nameof (SolidBrushViewModel.Color)));
        }

        public override void MouseUp(NSEvent theEvent)
        {
            base.MouseUp(theEvent);
			interaction?.Commit ();
			interaction = null;
        }

		protected override void OnPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			var inter = interaction ?? new EditorInteraction (ViewModel, null);

			switch (e.PropertyName) {
				case nameof (SolidBrushViewModel.Color):
				case nameof (SolidBrushViewModel.HueColor):	
				foreach (var editor in Layer.Sublayers.OfType<ColorEditorLayer> ()) {
					editor.UpdateFromModel (inter);
				}
					break;
			}
		}

		protected override void OnViewModelChanged (SolidBrushViewModel oldModel)
        {
            base.OnViewModelChanged(oldModel);
			var inter = interaction ?? new EditorInteraction (ViewModel, null);
			componentTabs.ViewModel = ViewModel;
				foreach (var editor in Layer.Sublayers.OfType<ColorEditorLayer> ()) {
				editor.UpdateFromModel (inter);
			}
		}

        public override void Layout ()
		{
			base.Layout ();

			if (Frame.IsEmpty || Frame.IsInfinite () || double.IsNaN (Frame.X) || double.IsInfinity (Frame.X))
				return;

			var secondarySpan = 20;
			var primarySpan = Frame.Height - 2 * padding - secondarySpan;
			var firstBase = padding;
			var secondBase = padding + secondarySpan;
			var firstStop = firstBase + primarySpan;

			Background.BorderColor = new CGColor (.5f, .5f, .5f, .5f);
			Background.BackgroundColor = NSColor.ControlBackground.CGColor;
			Background.Frame = new CGRect (0, 0, Frame.Height, Frame.Height);
			//Background.SetNeedsDisplay ();

			componentBackground.BorderColor = new CGColor (.5f, .5f, .5f, .5f);
			componentBackground.BackgroundColor = NSColor.ControlBackground.CGColor;
			componentBackground.Frame = new CGRect (0, 0, Frame.Height, Frame.Height);
			//componentBackground.SetNeedsDisplay ();
			var x = Frame.Height + 4 * padding;
			componentBackground.Frame = new CGRect (Frame.Height + 4 * padding, 0, Frame.Width - x, Frame.Height);

			Hue.Frame = new CGRect (firstStop, secondBase, secondarySpan, primarySpan);
			Hue.GripColor = NSColor.Text.CGColor;
			//Hue.SetNeedsDisplay ();
			Shade.Frame = new CGRect (firstBase, secondBase, primarySpan, primarySpan);
			History.Frame = new CGRect (firstBase, firstBase, primarySpan, secondarySpan);
			var inter = interaction ?? new EditorInteraction (ViewModel, null);
			foreach (var editor in Layer.Sublayers.OfType<ColorEditorLayer> ()) {
				editor.UpdateFromModel (inter);
			}

			componentTabs.View.Frame = componentBackground.Frame.Inset (4 * padding, 2 * padding);
		}
	}
}
