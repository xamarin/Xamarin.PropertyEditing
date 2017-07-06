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
	}
}