using System;
using AppKit;
using Foundation;
using CoreGraphics;

namespace Xamarin.PropertyEditing.Mac
{
	internal class AdaptingTextView : NSTextView
	{
		public AdaptingTextView ()
		{
			TextContainerInset = new CGSize (1, 4);
			Editable = true;
			FieldEditor = true;
			TranslatesAutoresizingMaskIntoConstraints = false;
		}

		public override void ChangeColor (NSObject sender)
		{
			//This override avoids a native automatic behaviour in NSColorPanel with a NSTextView
			//who changes the foreground color of the selected characters without code
		}

		public override CGSize IntrinsicContentSize {
			get {
				LayoutManager.EnsureLayoutForTextContainer (TextContainer);
				var size = LayoutManager.GetUsedRectForTextContainer (TextContainer).Size;
				size.Height += 8;
				return size;
			}
		}

		public override void DidChangeText ()
		{
			InvalidateIntrinsicContentSize ();
			base.DidChangeText ();
		}

		public override void DrawViewBackgroundInRect (CGRect rect)
		{
			base.DrawViewBackgroundInRect (rect);
			// TODO needed ?? DrawingUtils.DrawFullShadedBezel (Bounds, flipped: IsFlipped);
		}
	}
}
