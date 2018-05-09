using CoreAnimation;
using CoreGraphics;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.Mac
{
	class MaterialColorLayer : CATextLayer
	{
		public MaterialColorLayer ()
		{
			AddSublayer (selection);
		}

		readonly CATextLayer selection = new CATextLayer () {
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

		CommonColor backgroundColor;
		public new CommonColor BackgroundColor
		{
			get => backgroundColor;
			set
			{
				backgroundColor = value;
				base.BackgroundColor = backgroundColor.ToCGColor ();
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

			selection.String = text;
			selection.Frame = Bounds.Inset (3, 3);
			selection.BorderWidth = isSelected ? 2 : 0;
			selection.BorderColor = ForegroundColor;
			selection.ForegroundColor = ForegroundColor;
			selection.FontSize = FontSize;
			selection.ContentsScale = ContentsScale;
			selection.TextAlignmentMode = TextAlignmentMode;
		}
	}
}
