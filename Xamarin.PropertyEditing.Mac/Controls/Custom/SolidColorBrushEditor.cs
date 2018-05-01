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
		readonly ShadeLayer Shade = new ShadeLayer ();
		readonly HueLayer Hue = new HueLayer ();
		readonly HistoryLayer History = new HistoryLayer ();

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
