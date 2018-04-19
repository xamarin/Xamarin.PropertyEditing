using System;
using System.Collections.Generic;
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

		ComponentSet CreateEditor (ComponentEditor editor)
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
						CreateEditor (new HsbHueComponentEditor ()),
						CreateEditor (new HsbSaturationComponentEditor ()),
						CreateEditor (new HsbBrightnessComponentEditor ()),
						CreateEditor (new AlphaComponentEditor ())
					};
				case ChannelEditorType.HLS:
					return new[] {
						CreateEditor (new HlsHueComponentEditor ()),
						CreateEditor (new HlsLightnessComponentEditor ()),
						CreateEditor (new HlsSaturationComponentEditor ()),
						CreateEditor (new AlphaComponentEditor ())
					};
				case ChannelEditorType.RGB:
					return new[] {
						CreateEditor (new RedComponentEditor ()),
						CreateEditor (new GreenComponentEditor ()),
						CreateEditor (new BlueComponentEditor ()),
						CreateEditor (new AlphaComponentEditor ())
					};
				default:
				case ChannelEditorType.CMYK:
					return new[] {
						CreateEditor (new CyanComponentEditor ()),
						CreateEditor (new MagentaComponentEditor ()),
						CreateEditor (new YellowComponentEditor ()),
						CreateEditor (new BlackComponentEditor ()),
						CreateEditor (new AlphaComponentEditor ())
					};
			}
		}

		void Initialize ()
		{
			WantsLayer = true;
			Editors = CreateEditors (EditorType);

			this.DoConstraints (Editors.Select (
				ce => new[] {
					ce.Editor.ConstraintTo (this, (editor, c) => editor.Width == 90),
					ce.Editor.ConstraintTo (this, (editor, c) => editor.Height == DefaultControlHeight)
				}).SelectMany (e => e).ToArray ());	
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
						ViewModel.Color = channel.UpdateColorFromValue (
							ViewModel.Color,
							channel.ValueFromLocation (grad, Layer.ConvertPointToLayer (location,grad.SuperLayer)));
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
				ViewModel.Color = channel.UpdateColorFromValue (
					ViewModel.Color,
					channel.ValueFromLocation (grad, Layer.ConvertPointToLayer (location, grad.SuperLayer)));
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
			var frame = Frame.Bounds ().Border (new CommonThickness (padding));
			var labelFrame = new CGRect (frame.X, frame.Height - DefaultControlHeight, 20, DefaultControlHeight);
			var editorFrame = new CGRect (labelFrame.Right, labelFrame.Y, frame.Width - labelFrame.Right, DefaultControlHeight);
			var yOffset = DefaultControlHeight + DefaultGradientHeight + 3;

			foreach (var e in Editors) {
				e.Label.Frame = labelFrame;
				e.Editor.Frame = editorFrame;
				e.Gradient.Frame = new CGRect (editorFrame.X, editorFrame.Y - DefaultGradientHeight + 1, e.Editor.TextField.Frame.Width, DefaultGradientHeight);
				e.Gradient.BorderColor = NSColor.DisabledControlText.CGColor;
				e.Gradient.ContentsScale = Window?.Screen?.BackingScaleFactor ?? NSScreen.MainScreen.BackingScaleFactor;
				labelFrame = labelFrame.Translate (0, -yOffset);
				editorFrame = editorFrame.Translate (0, -yOffset);
			}
		}
	}

	public abstract class ComponentEditor
	{
		public string Name { get; }
		public double MinimumValue { get; }
		public double MaximumValue { get; }
		public double IncrementValue { get; }

		static IEnumerable<double> LerpSteps (double min, double max, int steps)
			=> Enumerable.Range (0, steps).Select (v => {
				var pos = v / (double)steps;
				return max * pos - min * (1 - pos);
			});

		public ComponentEditor (string name, double min, double max, double increment)
		{
			MinimumValue = min;
			MaximumValue = max;
			IncrementValue = increment;
			Name = name;
		}

		public void UpdateGradientLayer (CAGradientLayer layer, CommonColor color)
		{
			layer.Colors = LerpSteps (MinimumValue, MaximumValue, 7)
				.Select (value => UpdateColorFromValue (color.UpdateRGB (a: 255), value).ToCGColor ()).ToArray ();
		}

		public double InverseLerp (CGPoint start, CGPoint end, CGPoint loc)
		{
			var a = new CGVector (end.X - start.X, end.Y - start.Y);
			var b = new CGVector (loc.X - start.X, loc.Y - start.Y);
			var dot = a.dx * b.dx + a.dy * b.dy;
			var len = Math.Sqrt (a.dx * a.dx + a.dy * a.dy);
			var pos = dot / len;
			return MaximumValue * pos - MinimumValue * (1 - pos);
		}

		public CGPoint Lerp (CGPoint start, CGPoint end, double amount)
		{
			return new CGPoint (
				start.X + (end.X - start.X) * amount,
				start.Y + (end.Y - start.Y) * amount);
		}

		public double ValueFromLocation (CAGradientLayer layer, CGPoint loc)
		{
			var rect = layer.Frame;
			var unitLoc = new CGPoint (
				(loc.X - rect.X) / rect.Width,
				(loc.Y - rect.Y) / rect.Height);
			
			return Clamp (InverseLerp (layer.StartPoint, layer.EndPoint, unitLoc));
		}

		public CGPoint LocationFromColor (CAGradientLayer layer, CommonColor color)
		{
			var pos = ValueFromColor (color);
			var amount = (pos - MinimumValue) / (MaximumValue - MinimumValue);
			var unitLoc = Lerp (layer.StartPoint, layer.EndPoint, amount);

			return new CGPoint (
				layer.Frame.X + unitLoc.X * layer.Frame.Width,
			    layer.Frame.Y + unitLoc.Y * layer.Frame.Height);
		}

		public double Clamp (double value)
		=> Math.Max (MinimumValue, Math.Min (MaximumValue, value));

		public abstract CommonColor UpdateColorFromValue (CommonColor color, double value);
		public abstract double ValueFromColor (CommonColor color);
	}

	class ComponentSpinEditor : NumericSpinEditor
	{
		public ComponentSpinEditor (ComponentEditor component)
		{
			ComponentEditor = component;
			MinimumValue = component.MinimumValue;
			MaximumValue = component.MaximumValue;
			IncrementValue = component.IncrementValue;
		}

		public ComponentEditor ComponentEditor { get; }
	}

	class RedComponentEditor : ComponentEditor
	{
		public RedComponentEditor () : base ("R", 0d, 255d, 1d)
		{
		}

		public override double ValueFromColor (CommonColor color)
		=> (double)color.R;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateRGB (r: (byte)Clamp (value));
	}

	class GreenComponentEditor : ComponentEditor
	{
		public GreenComponentEditor () : base ("G", 0d, 255d, 1d)
		{
		}

		public override double ValueFromColor (CommonColor color)
		=> (double)color.G;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateRGB (g: (byte)Clamp (value));
	}

	class BlueComponentEditor : ComponentEditor
	{
		public BlueComponentEditor () : base ("B", 0d, 255d, 1d)
		{
		}

		public override double ValueFromColor (CommonColor color)
		=> (double)color.B;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateRGB (b: (byte)Clamp (value));
	}

	class AlphaComponentEditor : ComponentEditor
	{
		public AlphaComponentEditor () : base ("A", 0d, 255d, 1d)
		{
		}

		public override double ValueFromColor (CommonColor color)
		=> (double)color.A;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateRGB (a: (byte)Clamp (value));
	}

	class CyanComponentEditor : ComponentEditor 
	{
		public CyanComponentEditor () : base ("C", 0d, 1d, .01d)
		{
		}

		public override double ValueFromColor (CommonColor color)
		=> color.C;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateCMYK (c: Clamp (value));
	}

	class MagentaComponentEditor : ComponentEditor
	{
		public MagentaComponentEditor () : base ("M", 0d, 1d, .01d)
		{
		}

		public override double ValueFromColor (CommonColor color)
		=> color.M;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateCMYK (m: Clamp (value));
	}

	class YellowComponentEditor : ComponentEditor 
	{
		public YellowComponentEditor () : base ("Y", 0d, 1d, .01d)
		{
		}

		public override double ValueFromColor (CommonColor color)
		=> color.Y;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateCMYK (y: Clamp (value));
	}

	class BlackComponentEditor : ComponentEditor 
	{
		public BlackComponentEditor () : base ("K", 0d, 1d, .01d)
		{
		}

		public override double ValueFromColor (CommonColor color)
		=> color.K;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateCMYK (k: Clamp (value));
	}

	class HsbHueComponentEditor : ComponentEditor {
		public HsbHueComponentEditor () : base ("H", 0d, 360d, 1d)
		{
		}

		public override double ValueFromColor (CommonColor color)
		=> color.Hue;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateHSB (hue: Clamp (value));
	}

	class HsbSaturationComponentEditor : ComponentEditor
	{
		public HsbSaturationComponentEditor () : base ("S", 0d, 1d, .01d)
		{
		}

		public override double ValueFromColor (CommonColor color)
		=> color.Saturation;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateHSB (saturation: Clamp (value));
	}

	class HsbBrightnessComponentEditor : ComponentEditor
	{
		public HsbBrightnessComponentEditor () : base ("B", 0d, 1d, .01d)
		{
		}

		public override double ValueFromColor (CommonColor color)
		=> color.Brightness;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateHSB (brightness: Clamp (value));
	}

	class HlsHueComponentEditor : ComponentEditor
	{
		public HlsHueComponentEditor () : base ("H", 0d, 360d, 1d)
		{
		}

		public override double ValueFromColor (CommonColor color)
		=> color.Hue;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateHLS (hue: Clamp (value));
	}

	class HlsLightnessComponentEditor : ComponentEditor
	{
		public HlsLightnessComponentEditor () : base ("L", 0d, 1d, .01d)
		{
		}

		public override double ValueFromColor (CommonColor color)
		=> (double)color.Lightness;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateHLS (lightness: Clamp (value));
	}

	class HlsSaturationComponentEditor : ComponentEditor
	{
		public HlsSaturationComponentEditor () : base ("S", 0d, 1d, .01d)
		{
		}

		public override double ValueFromColor (CommonColor color)
		=> (double)color.Saturation;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateHLS (saturation: Clamp (value));
	}
}
