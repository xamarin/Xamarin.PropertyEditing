using System;
using AppKit;
using ObjCRuntime;

namespace Xamarin.PropertyEditing.Mac
{
	public interface IHostResourceProvider
	{
		NSAppearance GetVibrantAppearance (NSAppearance appearance);

		NSColor GetNamedColor (string name);
		NSImage GetNamedImage (string name);
		NSFont GetNamedFont (string name, nfloat fontSize);
	}
}
