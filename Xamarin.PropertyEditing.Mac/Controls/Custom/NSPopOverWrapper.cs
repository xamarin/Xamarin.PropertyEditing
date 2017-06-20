using System;
using System.Collections.Generic;
using System.Diagnostics;
using AppKit;
using Foundation;

namespace Xamarin.PropertyEditing.Mac
{
	public class NSPopoverWrapper : IPopover
	{
		// Always retain the object in a cache until it closes. This
		// guarantees that neither the NSPopover nor the native NSPopoverDelegate
		// can be GC'ed until after it has closed.
		static HashSet<IPopover> cache = new HashSet<IPopover> ();

		public event EventHandler Closed;

		public NSPopover popover;
		PopoverDelegate popoverDelegate;

		public NSPopoverWrapper (NSPopover popover)
		{
			this.popover = popover;
			this.popoverDelegate = new PopoverDelegate ();
			this.popover.Delegate = popoverDelegate;

			popoverDelegate.Closed += delegate {
				cache.Remove (this);
				if (Closed != null)
					Closed (this, EventArgs.Empty);
			};
		}

		public void Show (CoreGraphics.CGRect relativePositioningRect, NSView positioningView, NSRectEdge preferredEdge)
		{
			cache.Add (this);
			popover.ContentViewController.View.Appearance = positioningView.EffectiveAppearance;
			popover.Show (relativePositioningRect, positioningView, preferredEdge);
		}

		public void Close ()
		{
			popover.Close ();
		}

		void IDisposable.Dispose ()
		{
			// Do not call 'Dispose'. If we dispose here we could end up clearing the C# wrappers
			// for our obj-c objects before willClose/didClose events are raised by NSNotificationCenter.
			// This introduces a race where the object in NSPopover.Delegate might be completely cleaned
			// up before NSNotificationCenter invokes it, thus crashing the process.
			popover.Close ();
		}

		class PopoverDelegate : NSPopoverDelegate
		{
			public event EventHandler Closed;

			public override void DidClose (NSNotification notification)
			{
				try {
					if (Closed != null)
						Closed (this, EventArgs.Empty);
				}
				catch (Exception ex) {
					Debug.WriteLine ("Fatal error handling NSPopoverDelegate.DidClose. {0}", ex);
				}
			}
		}
	}
}
