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
	internal class EditorInteraction
	{
		public EditorInteraction (SolidBrushViewModel viewModel, ColorEditorLayer layer)
		{
			ViewModel = viewModel;
			Layer = layer;
		}

		public ColorEditorLayer Layer { get; set; }
		public SolidBrushViewModel ViewModel { get; set; }

		public CommonColor LastColor => ViewModel.LastColor;
		public CommonColor InitialColor => ViewModel.InitialColor;

		public CommonColor Color {
			get => ViewModel.Color;
			set => ViewModel.Color = value;
		}

		public void Commit ()
		{
			Layer.Commit (this);
		}
	}

	internal class SolidColorBrushEditor : ColorEditorView
	{
		public SolidColorBrushEditor (IntPtr handle) : base (handle)
		{
		}

		public SolidColorBrushEditor (CGRect frame) : base (frame)
		{
			Initialize ();
		}

		public SolidColorBrushEditor () : base ()
		{
			Initialize ();
		}

		private void Initialize ()
		{
			Layer = new CALayer ();
			Layer.AddSublayer (background);
			Layer.AddSublayer (shadeLayer);
			Layer.AddSublayer (hueLayer);
			Layer.AddSublayer (historyLayer);
			Layer.AddSublayer (componentBackground);
			WantsLayer = true;
			AddSubview (componentTabs.View);
		}

		private readonly ColorComponentTabViewController componentTabs = new ColorComponentTabViewController () {
			EditorType = ChannelEditorType.RGB
		};

		public override bool AcceptsFirstResponder () => true;

		private readonly ShadeLayer shadeLayer = new ShadeLayer ();
		private readonly HueLayer hueLayer = new HueLayer ();
		private readonly HistoryLayer historyLayer = new HistoryLayer ();

		private readonly CALayer background = new CALayer {
			CornerRadius = 3,
			BorderWidth = 1
		};

		private readonly CALayer componentBackground = new CALayer {
			CornerRadius = 3,
			BorderWidth = 1
		};

		private EditorInteraction interaction;
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

		private CGPoint last;
		public override void MouseDragged (NSEvent theEvent)
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

		public override void MouseUp (NSEvent theEvent)
		{
			base.MouseUp (theEvent);
			interaction?.Commit ();
			interaction = null;
		}

		private bool modelChanged = true;
		public override void OnPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			var inter = interaction ?? new EditorInteraction (ViewModel, null);

			switch (e.PropertyName) {
				case nameof (SolidBrushViewModel.Color):
				case nameof (SolidBrushViewModel.LastColor):
					this.modelChanged = NeedsLayout = true;
					break;
			}
		}

		public override void OnViewModelChanged (SolidBrushViewModel oldModel)
		{
			base.OnViewModelChanged (oldModel);
			var inter = interaction ?? new EditorInteraction (ViewModel, null);

			componentTabs.ViewModel = ViewModel;
			foreach (var editor in Layer.Sublayers.OfType<ColorEditorLayer> ()) {
				editor.UpdateFromModel (inter);
			}
		}

		public override void Layout ()
		{
			if (this.modelChanged) {
				var interx = interaction ?? new EditorInteraction (ViewModel, null);
				foreach (var editor in Layer.Sublayers.OfType<ColorEditorLayer> ()) {
					editor.UpdateFromModel (interx);
				}
				this.modelChanged = false;
			}

			base.Layout ();

			if (Frame.IsEmpty || Frame.IsInfinite () || double.IsNaN (Frame.X) || double.IsInfinity (Frame.X))
				return;

			var secondarySpan = 20;
			var primarySpan = Frame.Height - 2 * padding - secondarySpan;
			var firstBase = padding;
			var secondBase = padding + secondarySpan;
			var firstStop = firstBase + primarySpan;

			this.background.BorderColor = new CGColor (.5f, .5f, .5f, .5f);
			this.background.BackgroundColor = NSColor.ControlBackground.CGColor;
			this.background.Frame = new CGRect (0, 0, Frame.Height, Frame.Height);

			this.componentBackground.BorderColor = new CGColor (.5f, .5f, .5f, .5f);
			this.componentBackground.BackgroundColor = NSColor.ControlBackground.CGColor;
			this.componentBackground.Frame = new CGRect (0, 0, Frame.Height, Frame.Height);

			var x = firstStop + secondarySpan + 4 * padding;
			var backgroundFrame = new CGRect (x, 0, Math.Max(Frame.Width - x, 180), Frame.Height);
			this.componentBackground.Frame = backgroundFrame;

			this.hueLayer.Frame = new CGRect (firstStop, secondBase, secondarySpan, primarySpan);
			this.hueLayer.GripColor = NSColor.Text.CGColor;

			this.shadeLayer.Frame = new CGRect (firstBase, secondBase, primarySpan, primarySpan);
			this.historyLayer.Frame = new CGRect (firstBase, firstBase, primarySpan, secondarySpan);
			var inter = interaction ?? new EditorInteraction (ViewModel, null);
			foreach (var editor in Layer.Sublayers.OfType<ColorEditorLayer> ()) {
				editor.UpdateFromModel (inter);
			}

			var inset = backgroundFrame.Inset (4 * padding, 2 * padding);
			this.componentTabs.View.Frame = inset;
		}
	}
}
