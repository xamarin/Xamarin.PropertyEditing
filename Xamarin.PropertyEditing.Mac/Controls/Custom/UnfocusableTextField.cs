using System;
using System.Collections;
using System.ComponentModel;
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
			AccessibilityElement = false;
			Bordered = false;
			Cell.LineBreakMode = NSLineBreakMode.TruncatingTail;
			Cell.UsesSingleLineMode = true;
			ControlSize = NSControlSize.Small;
			Editable = false;
			Font = NSFont.FromFontName (PropertyEditorControl.DefaultFontName, PropertyEditorControl.DefaultPropertyLabelFontSize);
			Selectable = false;
		}

		public override void DrawRect (CGRect dirtyRect)
		{
			CGPoint origin = new CGPoint (0.0f, 4.0f);
			CGRect rect = new CGRect (origin, new CGSize (Bounds.Width - 5, Bounds.Height));

			this.AttributedStringValue.DrawInRect (rect);
		}
	}
}