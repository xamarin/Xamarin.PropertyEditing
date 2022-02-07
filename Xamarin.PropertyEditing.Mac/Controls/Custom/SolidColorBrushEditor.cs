using System;
using System.ComponentModel;
using System.Linq;

using AppKit;
using CoreAnimation;
using CoreGraphics;
using ObjCRuntime;

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

	internal class SolidColorBrushEditor
		: ColorEditorView
	{

		public SolidColorBrushEditor (IHostResourceProvider hostResources, CGRect frame)
			: base (frame)
		{
			Initialize (hostResources);
		}

		public SolidColorBrushEditor (IHostResourceProvider hostResources)
		{
			Initialize (hostResources);
		}

		private void Initialize (IHostResourceProvider hostResources)
		{
			this.historyLayer = new HistoryLayer (hostResources);

			this.componentTabs = new ColorComponentTabViewController (hostResources) {
				EditorType = ChannelEditorType.RGB
			};

			Layer = new CALayer ();
			Layer.AddSublayer (this.background);
			Layer.AddSublayer (this.shadeLayer);
			Layer.AddSublayer (this.hueLayer);
			Layer.AddSublayer (this.historyLayer);
			Layer.AddSublayer (this.componentBackground);
			WantsLayer = true;
			AddSubview (this.componentTabs.View);
		}

		public override bool AcceptsFirstResponder () => true;

		private readonly ShadeLayer shadeLayer = new ShadeLayer ();
		private readonly HueLayer hueLayer = new HueLayer ();
		private HistoryLayer historyLayer;
		private ColorComponentTabViewController componentTabs;

		private readonly CALayer background = new CALayer {
			CornerRadius = 3,
			BorderWidth = 1,
			BorderColor = new CGColor (.5f, .5f, .5f, .5f),
		};

		private readonly CALayer componentBackground = new CALayer {
			CornerRadius = 3,
			BorderWidth = 1,
			BorderColor = new CGColor (.5f, .5f, .5f, .5f)
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
						this.interaction = new EditorInteraction (ViewModel, active);
						active.UpdateFromLocation (
							this.interaction,
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

			this.interaction?.Layer?.UpdateFromLocation (
						this.interaction,
						Layer.ConvertPointToLayer (location, this.interaction.Layer));

			OnPropertyChanged (ViewModel, new PropertyChangedEventArgs (nameof (SolidBrushViewModel.Color)));
		}

		public override void MouseUp (NSEvent theEvent)
		{
			base.MouseUp (theEvent);
			this.interaction?.Commit ();
			this.interaction = null;
		}

		private bool modelChanged = true;

		public override void OnPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			var inter = this.interaction ?? new EditorInteraction (ViewModel, null);

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
			var inter = this.interaction ?? new EditorInteraction (ViewModel, null);

			this.componentTabs.ViewModel = ViewModel;
			foreach (var editor in Layer.Sublayers.OfType<ColorEditorLayer> ()) {
				editor.UpdateFromModel (inter);
			}
		}

		public override void Layout ()
		{
			if (this.modelChanged) {
				var interx = this.interaction ?? new EditorInteraction (ViewModel, null);
				foreach (var editor in Layer.Sublayers.OfType<ColorEditorLayer> ()) {
					editor.UpdateFromModel (interx);
				}
				this.modelChanged = false;
			}

			base.Layout ();

			if (Frame.IsEmpty || Frame.IsInfinite () || double.IsNaN (Frame.X) || double.IsInfinity (Frame.X))
				return;

			this.background.BackgroundColor = NSColor.ControlBackground.CGColor;
			this.componentBackground.BackgroundColor = NSColor.ControlBackground.CGColor;
			this.hueLayer.GripColor = NSColor.Text.CGColor;

			const float spacing = 8, hueWidth = 20, historyHeight = 20;
			const float leftMinWidth = hueWidth + (Padding * 2) + 50;
			const float rightWidth = 170;

			nfloat leftWidth = leftMinWidth;

			nfloat spaceLeft = Frame.Width - spacing - leftWidth - rightWidth;
			if (spaceLeft > 0) {
				leftWidth += spaceLeft;
			}

			nfloat vspace = Frame.Height - (Padding * 2);

			this.background.Frame = new CGRect (0, 0, leftWidth, Frame.Height);

			var shadeFrame = new CGRect (Padding, Padding + historyHeight + Padding, leftWidth - (Padding * 3) - hueWidth, vspace - historyHeight - Padding);
			this.shadeLayer.Frame = shadeFrame;
			this.historyLayer.Frame = new CGRect (Padding, Padding, shadeFrame.Width, historyHeight);
			this.hueLayer.Frame = new CGRect (this.shadeLayer.Frame.Right + Padding, this.historyLayer.Frame.Bottom + Padding, hueWidth, shadeFrame.Height);

			const float componentPadding = 9;
			var backgroundFrame = new CGRect (this.hueLayer.Frame.Right + spacing, 0, rightWidth, Frame.Height);
			this.componentBackground.Frame = backgroundFrame;
			var inset = backgroundFrame.Inset (componentPadding, componentPadding);
			this.componentTabs.View.Frame = inset;

			var inter = this.interaction ?? new EditorInteraction (ViewModel, null);
			foreach (var editor in Layer.Sublayers.OfType<ColorEditorLayer> ()) {
				editor.UpdateFromModel (inter);
			}
		}
	}
}
