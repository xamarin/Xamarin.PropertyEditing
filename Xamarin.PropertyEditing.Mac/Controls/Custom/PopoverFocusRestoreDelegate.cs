using System;
using System.Diagnostics;

using AppKit;
using Foundation;

namespace Xamarin.PropertyEditing.Mac
{
	internal class PopoverFocusRestoreDelegate : NSPopoverDelegate
	{
		static readonly NSString key = new NSString ("firstResponder");

		private NSResponder prevFirstResponder;
		private NSWindow window;

		public PopoverFocusRestoreDelegate ()
		{
		}

		public override void DidShow (NSNotification notification)
		{
			Debug.Assert (window == null);
			this.window = ((NSPopover)notification.Object).ContentViewController.View.Window;

			if (this.prevFirstResponder != null)
				window.MakeFirstResponder (this.prevFirstResponder);

			window.AddObserver (this, key, NSKeyValueObservingOptions.Initial | NSKeyValueObservingOptions.New, IntPtr.Zero);
		}

		public override void WillClose (NSNotification notification)
		{
			window.RemoveObserver (this, key);
			window = null;
		}

		public override void ObserveValue (NSString keyPath, NSObject ofObject, NSDictionary change, IntPtr context)
		{
			var window = ofObject as NSWindow;
			if (window != null && keyPath == key) {
				var firstResponder = change [ChangeNewKey] as NSResponder;
				if (firstResponder != null && !(firstResponder is NSWindow) && (!(firstResponder is NSView view) || view.Window == window))
					this.prevFirstResponder = ResolveResponder (firstResponder);
			} else {
				base.ObserveValue (keyPath, ofObject, change, context);
			}
		}

		// See first paragraph under "How the Field Editor Works"
		// https://developer.apple.com/library/archive/documentation/TextFonts/Conceptual/CocoaTextArchitecture/TextEditing/TextEditing.html#//apple_ref/doc/uid/TP40009459-CH3-SW29
		static NSResponder ResolveResponder (NSResponder responder)
		{
			if (responder is NSText text && text.FieldEditor)
				return (text.WeakDelegate as NSResponder) ?? responder;

			return responder;
		}
	}
}
