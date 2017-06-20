using System;
using AppKit;

namespace Xamarin.PropertyEditing.Mac
{
	public interface IPopover : IDisposable
	{
		event EventHandler Closed;

		void Close ();
		void Show (CoreGraphics.CGRect relativePositioningRect, NSView positioningView, NSRectEdge preferredEdge);
	}
}
