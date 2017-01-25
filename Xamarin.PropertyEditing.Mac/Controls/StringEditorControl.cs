using System;
using AppKit;
using CoreGraphics;

namespace Xamarin.PropertyEditing.Mac
{
	public class StringEditorControl : PropertyEditorControl
	{
		public StringEditorControl ()
		{
			StringEditor = new NSTextField (new CGRect (0, 0, 150, 20));
			StringEditor.BackgroundColor = NSColor.Clear;
			StringEditor.StringValue = "";
			AddSubview (StringEditor);
		}

		public NSTextField StringEditor { get; set; }
	}
}
