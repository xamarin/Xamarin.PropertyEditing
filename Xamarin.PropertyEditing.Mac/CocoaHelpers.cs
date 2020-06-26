//
// CocoaHelpers.cs
//
// Author:
//       jmedrano <josmed@microsoft.com>
//
// Copyright (c) 2020 
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using AppKit;

namespace Xamarin.PropertyEditing.Mac
{
	public static class CocoaHelpers
	{
		public static void RunModalForWindow (NSWindow window, NSView controlToFocusWhenWindowClosed, Action<NSModalResponse> responseHandler = null, int defaultDelayTime = 100)
		{
			//HACK: Because VS4Mac is a GTK application try force set to NSApplication.SharedApplication.RunModalForWindow 
			//breaks the current focused window. Try only focus the ID is not enought, because our IDE on get focus (gtk) will override the current
			//focused element, then launch a task to allow the IDE to get the focus and wait for synchcontext to focus the correct view.
			var	parentWindow = NSApplication.SharedApplication.KeyWindow;

			var result = (NSModalResponse)(int)NSApplication.SharedApplication.RunModalForWindow (window);

			//after run modal our FocusedWindow is null, we set the parent again
			parentWindow?.MakeKeyAndOrderFront (parentWindow);

			System.Threading.Tasks.Task.Delay (defaultDelayTime).ContinueWith (t => {
				responseHandler?.Invoke (result);
				parentWindow?.MakeFirstResponder (controlToFocusWhenWindowClosed);
			}, System.Threading.Tasks.TaskScheduler.FromCurrentSynchronizationContext ());
		}
	}
}
