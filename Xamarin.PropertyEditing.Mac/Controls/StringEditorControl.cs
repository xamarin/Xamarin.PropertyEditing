using System;
using AppKit;
using CoreGraphics;

namespace Xamarin.PropertyEditing.Mac
{
	public class StringEditorControl : PropertyEditorControl
	{
		public StringEditorControl ()
		{
			NSTextField stringEditor = new NSTextField (new CGRect (0,0,100,20));
			stringEditor.BackgroundColor = NSColor.Clear;
			AddSubview (stringEditor);
		}
	}
}
