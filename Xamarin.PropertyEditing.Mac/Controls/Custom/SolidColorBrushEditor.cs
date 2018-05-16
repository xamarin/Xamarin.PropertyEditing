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

	class SolidColorBrushEditor :ColorEditorView
	{
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
			Layer.AddSublayer (background);
			Layer.AddSublayer (shadeLayer);
			Layer.AddSublayer (hueLayer);
			Layer.AddSublayer (historyLayer);
			Layer.AddSublayer (componentBackground);
			WantsLayer = true;
			AddSubview (componentTabs.View);
		}

		readonly ColorComponentTabViewController componentTabs = new ColorComponentTabViewController () {
			EditorType = ChannelEditorType.RGB
		};

		public override bool AcceptsFirstResponder () => true;

		readonly ShadeLayer shadeLayer = new ShadeLayer ();
		readonly HueLayer hueLayer = new HueLayer ();
		readonly HistoryLayer historyLayer = new HistoryLayer ();

		readonly CALayer background = new CALayer {
			CornerRadius = 3,
			BorderWidth = 1
		};

		readonly CALayer componentBackground = new CALayer {
			CornerRadius = 3,
			BorderWidth = 1
		};

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

		bool modelChanged = true;
		protected override void OnPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			var inter = interaction ?? new EditorInteraction (ViewModel, null);

			switch (e.PropertyName) {
				case nameof (SolidBrushViewModel.Color):
				case nameof (SolidBrushViewModel.LastColor):
					modelChanged = NeedsLayout = true;
					break;
			}
		}

		protected override void OnViewModelChanged (SolidBrushViewModel oldModel)
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
			if (modelChanged) {
				var interx = interaction ?? new EditorInteraction (ViewModel, null);
				foreach (var editor in Layer.Sublayers.OfType<ColorEditorLayer> ()) {
					editor.UpdateFromModel (interx);
				}
				modelChanged = false;
			}

			base.Layout ();

			if (Frame.IsEmpty || Frame.IsInfinite () || double.IsNaN (Frame.X) || double.IsInfinity (Frame.X))
				return;

			var secondarySpan = 20;
			var primarySpan = Frame.Height - 2 * padding - secondarySpan;
			var firstBase = padding;
			var secondBase = padding + secondarySpan;
			var firstStop = firstBase + primarySpan;

			background.BorderColor = new CGColor (.5f, .5f, .5f, .5f);
			background.BackgroundColor = NSColor.ControlBackground.CGColor;
			background.Frame = new CGRect (0, 0, Frame.Height, Frame.Height);

			componentBackground.BorderColor = new CGColor (.5f, .5f, .5f, .5f);
			componentBackground.BackgroundColor = NSColor.ControlBackground.CGColor;
			componentBackground.Frame = new CGRect (0, 0, Frame.Height, Frame.Height);

			var x = Frame.Height + 4 * padding;
			componentBackground.Frame = new CGRect (Frame.Height + 4 * padding, 0, Frame.Width - x, Frame.Height);

			hueLayer.Frame = new CGRect (firstStop, secondBase, secondarySpan, primarySpan);
			hueLayer.GripColor = NSColor.Text.CGColor;

			shadeLayer.Frame = new CGRect (firstBase, secondBase, primarySpan, primarySpan);
			historyLayer.Frame = new CGRect (firstBase, firstBase, primarySpan, secondarySpan);
			var inter = interaction ?? new EditorInteraction (ViewModel, null);
			foreach (var editor in Layer.Sublayers.OfType<ColorEditorLayer> ()) {
				editor.UpdateFromModel (inter);
			}

			componentTabs.View.Frame = componentBackground.Frame.Inset (4 * padding, 2 * padding);
		}
	}
}
