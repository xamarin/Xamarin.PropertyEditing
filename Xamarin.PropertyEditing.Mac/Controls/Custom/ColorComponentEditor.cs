using System;
using System.ComponentModel;
using System.Linq;
using AppKit;
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

		EditorType EditorType { get; }

		public ColorComponentEditor (EditorType editorType, CGRect frame) : base (frame)
		{
			EditorType = EditorType;
			Initialize ();
		}

		public ColorComponentEditor (EditorType editorType) : base ()
		{
			EditorType = editorType;
			Initialize ();
		}

		class ComponentSet
		{
			public UnfocusableTextField Label { get; set; }
			public ComponentEditor Editor { get; set; }
		}

		ComponentSet CreateEditor (string label, double min, double max, double increment, ComponentEditor editor)
		{
			var ce = new ComponentSet {
				Label = new UnfocusableTextField { StringValue = label },
				Editor = editor,
			};
			editor.MinimumValue = min;
			editor.MaximumValue = max;
			editor.IncrementValue = increment;
			editor.ValueChanged += UpdateComponent;
			AddSubview (ce.Label);
			AddSubview (editor);
			return ce;
		}

		ComponentSet[] Editors { get; set; }

		ComponentSet[] CreateEditors (EditorType type)
		{
			switch (type) {
				case EditorType.Hsb:
					return new[] {
						CreateEditor ("H:", 0, 360, 1, new HsbHueComponentEditor ()),
						CreateEditor ("S:", 0, 1, .01, new HsbSaturationComponentEditor ()),
						CreateEditor ("B:", 0, 1, .01, new HsbBrightnessComponentEditor ()),
						CreateEditor ("A:", 0, 255, 1, new AlphaComponentEditor ())
					};
				case EditorType.Hls:
					return new[] {
						CreateEditor ("H:", 0, 360, 1, new HlsHueComponentEditor ()),
						CreateEditor ("L:", 0, 1, .01, new HlsLightnessComponentEditor ()),
						CreateEditor ("S:", 0, 1, .01, new HlsSaturationComponentEditor ()),
						CreateEditor ("A:", 0, 255, 1, new AlphaComponentEditor ())
					};
				case EditorType.Rgb:
					return new[] {
						CreateEditor ("R:", 0, 255, 1, new RedComponentEditor ()),
						CreateEditor ("G:", 0, 255, 1, new GreenComponentEditor ()),
						CreateEditor ("B:", 0, 255, 1, new BlueComponentEditor ()),
						CreateEditor ("A:", 0, 255, 1, new AlphaComponentEditor ())
					};
				default:
				case EditorType.Cmyk:
					return new[] {
						CreateEditor ("C:", 0, 1, .01, new CyanComponentEditor ()),
						CreateEditor ("M:", 0, 1, .01, new MagentaComponentEditor ()),
						CreateEditor ("Y:", 0, 1, .01, new YellowComponentEditor ()),
						CreateEditor ("K:", 0, 1, .01, new BlackComponentEditor ()),
						CreateEditor ("A:", 0, 255, 1, new AlphaComponentEditor ())
					};
			}
		}

		void Initialize ()
		{
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
			var editor = sender as ComponentEditor;
			ViewModel.Color = editor.UpdateColorFromValue (color, editor.Value);
		}

        protected override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(sender, e);
			switch (e.PropertyName) {
				case nameof (SolidBrushViewModel.Color):
					foreach (var c in Editors) {
						var editor = c.Editor;
						editor.Value = editor.ValueFromColor(ViewModel.Color);
					}
				break;
			}
        }

        public override void Layout ()
		{
			base.Layout ();

			var frame = Frame.Bounds ().Border (new CommonThickness (padding));
			var labelFrame = new CGRect (frame.X, frame.Height - 22, 20, DefaultControlHeight);
			var editorFrame = new CGRect (labelFrame.X + labelFrame.Width, labelFrame.Y, 90, DefaultControlHeight);

			foreach (var e in Editors) {
				e.Label.Frame = labelFrame;
				e.Editor.Frame = editorFrame;
				labelFrame = labelFrame.Translate (0, -(labelFrame.Height + 3));
				editorFrame = editorFrame.Translate (0, -(labelFrame.Height + 3));
			}
		}
	}

	public abstract class ComponentEditor : NumericSpinEditor
	{
		public abstract CommonColor UpdateColorFromValue (CommonColor color, double value);
		public abstract double ValueFromColor (CommonColor color);
	}

	class RedComponentEditor : ComponentEditor
	{
		public override double ValueFromColor (CommonColor color)
		=> (double)color.R;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateRGB (r: (byte)value);
	}

	class GreenComponentEditor : ComponentEditor
	{
		public override double ValueFromColor (CommonColor color)
		=> (double)color.G;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateRGB (g: (byte)value);
	}

	class BlueComponentEditor : ComponentEditor
	{
		public override double ValueFromColor (CommonColor color)
		=> (double)color.B;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateRGB (b: (byte)value);
	}

	class AlphaComponentEditor : ComponentEditor
	{
		public override double ValueFromColor (CommonColor color)
		=> (double)color.A;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateRGB (a: (byte)value);
	}

	class CyanComponentEditor : ComponentEditor 
	{
		public override double ValueFromColor (CommonColor color)
		=> (double)color.C;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateCMYK (c: value);
	}

	class MagentaComponentEditor : ComponentEditor
	{
		public override double ValueFromColor (CommonColor color)
		=> (double)color.M;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateCMYK (m: value);
	}

	class YellowComponentEditor : ComponentEditor 
	{
		public override double ValueFromColor (CommonColor color)
		=> (double)color.Y;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateCMYK (y: value);
	}

	class BlackComponentEditor : ComponentEditor 
	{
		public override double ValueFromColor (CommonColor color)
		=> (double)color.K;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateCMYK (k: value);
	}

	class HsbHueComponentEditor : ComponentEditor {
		public override double ValueFromColor (CommonColor color)
		=> (double)color.Hue;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateHSB (hue: value);
	}

	class HsbSaturationComponentEditor : ComponentEditor
	{
		public override double ValueFromColor (CommonColor color)
		=> (double)color.Saturation;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateHSB (saturation: value);
	}

	class HsbBrightnessComponentEditor : ComponentEditor
	{
		public override double ValueFromColor (CommonColor color)
		=> (double)color.Brightness;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateHSB (brightness: value);
	}

	class HlsHueComponentEditor : ComponentEditor
	{
		public override double ValueFromColor (CommonColor color)
		=> (double)color.Hue;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateHLS (hue: value);
	}

	class HlsLightnessComponentEditor : ComponentEditor
	{
		public override double ValueFromColor (CommonColor color)
		=> (double)color.Lightness;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateHLS (lightness: value);
	}

	class HlsSaturationComponentEditor : ComponentEditor
	{
		public override double ValueFromColor (CommonColor color)
		=> (double)color.Saturation;

		public override CommonColor UpdateColorFromValue (CommonColor color, double value)
		=> color.UpdateHLS (saturation: value);
	}
}
