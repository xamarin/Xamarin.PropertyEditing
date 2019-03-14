using System;
using AppKit;
using CoreAnimation;
using CoreGraphics;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.Mac
{
	public enum MaterialColorType
	{
		Palette,
		Normal,
		Accent
	}

	class MaterialColorLayer : NSView
	{
		public MaterialColorLayer ()
		{
			Initialize ();
		}

		private void Initialize ()
		{
			WantsLayer = true;
		}

		private readonly CATextLayer selection = new CATextLayer () {
			CornerRadius = 3
		};

		public MaterialColorType ColorType { get; set; } = MaterialColorType.Palette;

		public CommonColor BackgroundColor { get; set; }

		public CGColor ForegroundColor { get; set; }

		public CGColor BorderColor { get; set; }

		public int FontSize { get; set; }

		public int CornerRadius { get; set; }

		public bool MasksToBounds { get; set; }

		public nfloat ContentsScale { get; set; }

		public CATextLayerAlignmentMode TextAlignmentMode { get; set; }

		private string text;
		public string Text {
			get => this.text;
			set {
				this.text = value;
				NeedsLayout = true;
			}
		}

		private bool isSelected;
		public bool IsSelected {
			get => this.isSelected;
			set {
				if (this.isSelected == value)
					return;
				this.isSelected = value;
				NeedsLayout = true;
			}
		}

		public override void Layout ()
		{
			this.selection.String = this.text;
			this.selection.Frame = Bounds.Inset (3, 3);
			this.selection.BorderWidth = this.isSelected ? 2 : 0;
			this.selection.BackgroundColor = BackgroundColor.ToCGColor ();
			this.selection.BorderColor = ForegroundColor;
			this.selection.ForegroundColor = ForegroundColor;
			this.selection.FontSize = FontSize;
			this.selection.ContentsScale = ContentsScale;
			this.selection.TextAlignmentMode = TextAlignmentMode;
		}
	}
}
