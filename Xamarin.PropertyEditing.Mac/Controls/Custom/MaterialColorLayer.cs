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

	class MaterialColorLayer : CATextLayer
	{
		public MaterialColorLayer ()
		{
			AddSublayer (this.selection);
		}

		private readonly CATextLayer selection = new CATextLayer () {
			CornerRadius = 3
		};

		public MaterialColorType ColorType { get; set; } = MaterialColorType.Palette;

		private string text;
		public string Text {
			get => this.text;
			set {
				this.text = value;
				SetNeedsLayout ();
			}
		}

		private CommonColor backgroundColor;
		public new CommonColor BackgroundColor {
			get => this.backgroundColor;
			set {
				this.backgroundColor = value;
				base.BackgroundColor = this.backgroundColor.ToCGColor ();
			}
		}

		private bool isSelected;
		public bool IsSelected {
			get => this.isSelected;
			set {
				if (this.isSelected == value)
					return;
				this.isSelected = value;
				SetNeedsLayout ();
			}
		}

		public override void LayoutSublayers ()
		{
			base.LayoutSublayers ();

			this.selection.String = this.text;
			this.selection.Frame = Bounds.Inset (3, 3);
			this.selection.BorderWidth = this.isSelected ? 2 : 0;
			this.selection.BorderColor = ForegroundColor;
			this.selection.ForegroundColor = ForegroundColor;
			this.selection.FontSize = FontSize;
			this.selection.ContentsScale = ContentsScale;
			this.selection.TextAlignmentMode = TextAlignmentMode;
		}
	}
}
