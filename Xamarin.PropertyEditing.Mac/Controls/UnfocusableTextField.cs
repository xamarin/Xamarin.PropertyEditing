using System;
using AppKit;
using CoreGraphics;

namespace Xamarin.PropertyEditing.Mac
{
	class UnfocusableTextField : NSTextField
	{
		public UnfocusableTextField () : base ()
		{
			SetDefaultProperties ();
		}

		public UnfocusableTextField (CGRect frameRect, string text) : base (frameRect)
		{
			StringValue = text;
			SetDefaultProperties ();
		}

		void SetDefaultProperties ()
		{
			Bordered = false;
			Editable = false;
			Selectable = false;
		}

		public override void DrawRect (CGRect dirtyRect)
		{
			CGPoint origin = new CGPoint (0.0f, 7.0f);
			CGRect rect = new CGRect (origin, new CGSize (this.Bounds.Width, this.Bounds.Height));

			this.AttributedStringValue.DrawInRect (rect);
		}
	}
}