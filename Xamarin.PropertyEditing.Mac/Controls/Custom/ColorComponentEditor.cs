using System;
using System.ComponentModel;
using System.Linq;
using AppKit;
using CoreAnimation;
using CoreGraphics;
using Xamarin.PropertyEditing.Drawing;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	class ColorComponentEditor : ColorEditorView
	{
		const int DefaultPropertyButtonSize = 10;
		const int DefaultActioButtonSize = 16;
		const int DefaultControlHeight = 22;
		const int DefaultGradientHeight = 4;

		ChannelEditorType EditorType { get; }

		public bool ClickableGradients { get; set; } = true;

		public ColorComponentEditor (ChannelEditorType editorType, CGRect frame) : base (frame)
		{
			EditorType = EditorType;
			Initialize ();
		}

		public ColorComponentEditor (ChannelEditorType editorType) : base ()
		{
			EditorType = editorType;
			Initialize ();
		}

		ChannelGroup [] Editors { get; set; }
		UnfocusableTextField hexLabel;
		NSTextField hexEditor;

		class ChannelGroup
		{
			public UnfocusableTextField Label { get; set; }
			public ComponentSpinEditor Editor { get; set; }
			public CAGradientLayer Gradient { get; set; }
		}

		ChannelGroup CreateEditor (ChannelEditor editor)
		{
			var ce = new ChannelGroup {
				Label = new UnfocusableTextField {
					StringValue = $"{editor.Name}:",
					Alignment = NSTextAlignment.Right
				},
				Editor = new ComponentSpinEditor (editor),
				Gradient = new CAGradientLayer {
					StartPoint = new CGPoint (0, 0),
					EndPoint = new CGPoint (1, 0),
					BorderWidth = .5f,
				}
			};

			ce.Editor.TranslatesAutoresizingMaskIntoConstraints = true;
			ce.Editor.ValueChanged += UpdateComponent;
			ce.Editor.BackgroundColor = NSColor.Clear;
			ce.Label.BackgroundColor = NSColor.Clear;
			AddSubview (ce.Label);
			AddSubview (ce.Editor);

			Layer.AddSublayer (ce.Gradient);
			return ce;
		}

		ChannelGroup [] CreateEditors (ChannelEditorType type)
		{

			switch (type) {
				case ChannelEditorType.HSB:
					return new [] {
						CreateEditor (new HsbHueChannelEditor ()),
						CreateEditor (new HsbSaturationChannelEditor ()),
						CreateEditor (new HsbBrightnessChannelEditor ()),
						CreateEditor (new AlphaChannelEditor ())
					};
				case ChannelEditorType.HLS:
					return new [] {
						CreateEditor (new HlsHueChannelEditor ()),
						CreateEditor (new HlsLightnessChannelEditor ()),
						CreateEditor (new HlsSaturationChannelEditor ()),
						CreateEditor (new AlphaChannelEditor ())
					};
				case ChannelEditorType.RGB:
					return new [] {
						CreateEditor (new RedChannelEditor ()),
						CreateEditor (new GreenChannelEditor ()),
						CreateEditor (new BlueChannelEditor ()),
						CreateEditor (new AlphaChannelEditor ())
					};
				default:
				case ChannelEditorType.CMYK:
					return new [] {
						CreateEditor (new CyanChannelEditor ()),
						CreateEditor (new MagentaChannelEditor ()),
						CreateEditor (new YellowChannelEditor ()),
						CreateEditor (new BlackChannelEditor ()),
						CreateEditor (new AlphaChannelEditor ())
					};
			}
		}

		void Initialize ()
		{
			WantsLayer = true;
			Editors = CreateEditors (EditorType);

			hexLabel = new UnfocusableTextField {
				StringValue = "#:",
				Alignment = NSTextAlignment.Right,
				BackgroundColor = NSColor.Clear
			};
			AddSubview (hexLabel);

			hexEditor = new NSTextField {
				Alignment = NSTextAlignment.Right,
				BackgroundColor = NSColor.Clear
			};
			AddSubview (hexEditor);

			hexEditor.EditingEnded += (o, e) => {
				if (CommonColor.TryParseArgbHex (hexEditor.StringValue, out CommonColor c)) {
					ViewModel.Color = c;
					hexEditor.StringValue = c.ToString ();
				}
			};
		}

		void UpdateComponent (object sender, EventArgs args)
		{
			if (ViewModel == null)
				return;

			var color = ViewModel.Color;
			var editor = sender as ComponentSpinEditor;
			ViewModel.Color = editor.ComponentEditor.UpdateColorFromValue (color, editor.Value);
			ViewModel.CommitLastColor ();
		}

		protected override void OnPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			base.OnPropertyChanged (sender, e);

			switch (e.PropertyName) {
				case nameof (SolidBrushViewModel.Color):
					foreach (var channelGroup in Editors) {
						var editor = channelGroup.Editor;
						editor.Value = editor.ComponentEditor.ValueFromColor (ViewModel.Color);
						editor.ComponentEditor.UpdateGradientLayer (channelGroup.Gradient, ViewModel.Color);
					}
					hexEditor.StringValue = ViewModel.Color.ToString ();
					break;
			}
		}

		public override void UpdateConstraints ()
		{
			base.UpdateConstraints ();
		}

		ChannelGroup activeChannel;
		public override void MouseDown (NSEvent theEvent)
		{
			if (!ClickableGradients) {
				activeChannel = null;
				base.MouseDown (theEvent);
				return;
			}

			var location = ConvertPointFromView (theEvent.LocationInWindow, null);
			location = ConvertPointToLayer (location);

			foreach (var layer in Layer.Sublayers) {
				var hit = layer.PresentationLayer.HitTest (location) ?? layer.PresentationLayer.HitTest (new CGPoint (location.X, location.Y + 4));

				for (var currentLayer = hit; currentLayer != null; currentLayer = currentLayer.SuperLayer) {
					activeChannel = Editors.FirstOrDefault (ce => ce.Gradient == currentLayer.ModelLayer);
					if (activeChannel != null) {
						var channel = activeChannel.Editor.ComponentEditor;
						var grad = activeChannel.Gradient;
						ViewModel.Color = channel.UpdateColorFromLocation (
							grad,
							ViewModel.Color,
							Layer.ConvertPointToLayer (location, grad.SuperLayer));
						return;
					}
				}
			}
			base.MouseDown (theEvent);
		}

		public override void MouseDragged (NSEvent theEvent)
		{
			var location = ConvertPointFromView (theEvent.LocationInWindow, null);
			location = ConvertPointToLayer (location);

			if (activeChannel != null) {
				var channel = activeChannel.Editor.ComponentEditor;
				var grad = activeChannel.Gradient;
				ViewModel.Color = channel.UpdateColorFromLocation (
					grad,
					ViewModel.Color,
					Layer.ConvertPointToLayer (location, grad.SuperLayer));
				return;
			}
			base.MouseMoved (theEvent);
		}

		public override void MouseUp (NSEvent theEvent)
		{
			if (activeChannel != null)
				ViewModel.CommitLastColor ();

			activeChannel = null;
			base.MouseUp (theEvent);
		}

		public override void Layout ()
		{
			base.Layout ();

			var frame = Bounds.Inset (padding, padding);
			var labelFrame = new CGRect (frame.X, frame.Height - DefaultControlHeight, 20, DefaultControlHeight);
			var editorFrame = new CGRect (labelFrame.Right, labelFrame.Y, frame.Width - labelFrame.Right, DefaultControlHeight);
			var yOffset = DefaultControlHeight + DefaultGradientHeight + 3;

			foreach (var channelGroup in Editors) {
				channelGroup.Label.Frame = labelFrame;
				channelGroup.Editor.Frame = editorFrame;
				channelGroup.Gradient.Frame = new CGRect (
					editorFrame.X,
					editorFrame.Y - DefaultGradientHeight + 1,
					editorFrame.Width - 16, DefaultGradientHeight);

				channelGroup.Gradient.BorderColor = NSColor.DisabledControlText.CGColor;
				channelGroup.Gradient.ContentsScale = Window?.Screen?.BackingScaleFactor ?? NSScreen.MainScreen.BackingScaleFactor;
				labelFrame = labelFrame.Translate (0, -yOffset);
				editorFrame = editorFrame.Translate (0, -yOffset);
			}

			hexLabel.Frame = new CGRect (frame.X, padding, 20, DefaultControlHeight);
			hexEditor.Frame = new CGRect (
				labelFrame.Right,
				padding,
				frame.Width - labelFrame.Right - 16,
				DefaultControlHeight);
		}
	}
}
