using System;
using AppKit;
using Foundation;

namespace Xamarin.PropertyEditing.Mac
{
	internal class ModalWindowCloseDelegate : NSWindowDelegate
	{
		public NSModalResponse Response { get; set; } = NSModalResponse.Cancel;

		public override void WillClose (NSNotification notification)
		{
			NSApplication.SharedApplication.StopModalWithCode ((int)Response);
		}
	}
}
