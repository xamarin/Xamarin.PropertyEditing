using CoreAnimation;
using CoreGraphics;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.Mac
{
	class MaterialColorLayer : CATextLayer
	{
		public MaterialColorLayer ()
		{
			AddSublayer (Selection);
		}

		readonly CATextLayer Selection = new CATextLayer () {
			CornerRadius = 3
		};

		string text;
		public string Text
		{
			get => text;
			set
			{
				text = value;
				SetNeedsLayout ();
			}
		}

		CommonColor materialColor;
		public new CommonColor BackgroundColor
		{
			get => materialColor;
			set
			{
				materialColor = value;
				base.BackgroundColor = materialColor.ToCGColor ();
			}
		}

		bool isSelected;
		public bool IsSelected
		{
			get => isSelected;
			set
			{
				if (isSelected == value)
					return;
				isSelected = value;
				SetNeedsLayout ();
			}
		}

		public override void LayoutSublayers ()
		{
			base.LayoutSublayers ();

			Selection.String = text;
			Selection.Frame = Bounds.Inset (3, 3);
			Selection.BorderWidth = isSelected ? 2 : 0;
			Selection.BorderColor = ForegroundColor;
			Selection.ForegroundColor = ForegroundColor;
			Selection.FontSize = FontSize;
			Selection.ContentsScale = ContentsScale;
			Selection.TextAlignmentMode = TextAlignmentMode;
		}
	}
}
