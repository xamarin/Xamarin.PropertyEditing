using System;
using AppKit;
using CoreGraphics;

namespace Xamarin.PropertyEditing.Mac
{
	class UnfocusableTextView : NSTextView
	{
		public UnfocusableTextView () : base ()
		{
			SetDefaultProperties ();
		}

		public UnfocusableTextView (CGRect frameRect, string text) : base (frameRect)
		{
			Value = text;
			SetDefaultProperties ();
		}

		void SetDefaultProperties ()
		{
			Editable = false;
			Selectable = false;
		}
	}
}