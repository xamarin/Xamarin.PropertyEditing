using System;
using AppKit;

namespace Xamarin.PropertyEditing.Mac
{
	public class PropertyEditorControl : NSView
	{
		public PropertyEditorControl ()
		{
		}

		public string Label { get; set; }
	}
}
