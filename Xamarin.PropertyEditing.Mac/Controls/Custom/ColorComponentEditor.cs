using System;
using System.ComponentModel;
using System.Linq;
using AppKit;
using CoreAnimation;
using CoreGraphics;
using Foundation;
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

		class ComponentSet
		{
			public UnfocusableTextField Label { get; set; }
			public ComponentSpinEditor Editor { get; set; }
			public CAGradientLayer Gradient { get; set; }
		}

		ComponentSet CreateEditor (ChannelEditor editor)
		{
			var ce = new ComponentSet {
				Label = new UnfocusableTextField {
					StringValue = $"{editor.Name}:",
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
			AddSubview (ce.Label);
			AddSubview (ce.Editor);

			Layer.AddSublayer (ce.Gradient);
			return ce;
		}

		ComponentSet[] Editors { get; set; }

		ComponentSet[] CreateEditors (ChannelEditorType type)
		{
			switch (type) {
				case ChannelEditorType.HSB:
					return new[] {
						CreateEditor (new HsbHueChannelEditor ()),
						CreateEditor (new HsbSaturationChannelEditor ()),
						CreateEditor (new HsbBrightnessChannelEditor ()),
						CreateEditor (new AlphaChannelEditor ())
					};
				case ChannelEditorType.HLS:
					return new[] {
						CreateEditor (new HlsHueChannelEditor ()),
						CreateEditor (new HlsLightnessChannelEditor ()),
						CreateEditor (new HlsSaturationChannelEditor ()),
						CreateEditor (new AlphaChannelEditor ())
					};
				case ChannelEditorType.RGB:
					return new[] {
						CreateEditor (new RedChannelEditor ()),
						CreateEditor (new GreenChannelEditor ()),
						CreateEditor (new BlueChannelEditor ()),
						CreateEditor (new AlphaChannelEditor ())
					};
				default:
				case ChannelEditorType.CMYK:
					return new[] {
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
		}

        void UpdateComponent (object sender, EventArgs args)
		{
			if (ViewModel == null)
				return;

			var color = ViewModel.Color;
			var editor = sender as ComponentSpinEditor;
			ViewModel.Color = editor.ComponentEditor.UpdateColorFromValue (color, editor.Value);
		}

        protected override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(sender, e);
			switch (e.PropertyName) {
				case nameof (SolidBrushViewModel.Color):
					foreach (var c in Editors) {
						var editor = c.Editor;
						editor.Value = editor.ComponentEditor.ValueFromColor(ViewModel.Color);
						editor.ComponentEditor.UpdateGradientLayer (c.Gradient, ViewModel.Color);
					}
				break;
			}
        }

        public override void UpdateConstraints()
        {
            base.UpdateConstraints();
        }

        ComponentSet set;
		public override void MouseDown (NSEvent theEvent)
		{
			if (!ClickableGradients) {
				set = null;
				base.MouseDown (theEvent);
				return;
			}

			var location = ConvertPointFromView (theEvent.LocationInWindow, null);
			location = ConvertPointToLayer (location);

			foreach (var layer in Layer.Sublayers) {
				var hit = layer.PresentationLayer.HitTest (location) ?? layer.PresentationLayer.HitTest (new CGPoint (location.X, location.Y + 4));

				for (var c = hit; c != null; c = c.SuperLayer) {
					set = Editors.FirstOrDefault (ce => ce.Gradient == c.ModelLayer);
					if (set != null) {
						var channel = set.Editor.ComponentEditor;
						var grad = set.Gradient;
						ViewModel.Color = channel.UpdateColorFromLocation (
							grad,
							ViewModel.Color,
							Layer.ConvertPointToLayer (location,grad.SuperLayer));
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

			if (set != null) {
				var channel = set.Editor.ComponentEditor;
				var grad = set.Gradient;
				ViewModel.Color = channel.UpdateColorFromLocation (
					grad,
					ViewModel.Color,
					Layer.ConvertPointToLayer (location, grad.SuperLayer));
				return;
			}
			base.MouseMoved (theEvent);
		}

        public override void MouseUp(NSEvent theEvent)
        {
			set = null;
            base.MouseUp(theEvent); 
        }

        public override void Layout ()
		{
			base.Layout ();

			var frame = Bounds.Inset (padding, padding);
			var labelFrame = new CGRect (frame.X, frame.Height - DefaultControlHeight, 20, DefaultControlHeight);
			var editorFrame = new CGRect (labelFrame.Right, labelFrame.Y, frame.Width - labelFrame.Right, DefaultControlHeight);
			var yOffset = DefaultControlHeight + DefaultGradientHeight + 3;

			foreach (var e in Editors) {
				e.Label.Frame = labelFrame;
				e.Editor.Frame = editorFrame;
				e.Gradient.Frame = new CGRect (editorFrame.X, editorFrame.Y - DefaultGradientHeight, editorFrame.Width - 16, DefaultGradientHeight);
				e.Gradient.BorderColor = NSColor.DisabledControlText.CGColor;
				e.Gradient.ContentsScale = Window?.Screen?.BackingScaleFactor ?? NSScreen.MainScreen.BackingScaleFactor;
				labelFrame = labelFrame.Translate (0, -yOffset);
				editorFrame = editorFrame.Translate (0, -yOffset);
			}
		}
	}

	class ComponentSpinEditor : NumericSpinEditor
	{
		public ComponentSpinEditor (ChannelEditor component)
		{
			ComponentEditor = component;
			MinimumValue = component.MinimumValue;
			MaximumValue = component.MaximumValue;
			IncrementValue = component.IncrementValue;
			Digits = 2;
		}

		public ChannelEditor ComponentEditor { get; }
	}
}
