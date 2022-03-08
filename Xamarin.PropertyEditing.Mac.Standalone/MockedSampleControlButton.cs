using System;
using Foundation;
using ObjCRuntime;
using Xamarin.PropertyEditing.Tests.MockControls;

namespace Xamarin.PropertyEditing.Mac.Standalone
{
	[Foundation.Register ("MockedSampleControlButton")]
	public class MockedSampleControlButton : MockedControlButton<MockSampleControl>
	{
		// Called when created from unmanaged code
		public MockedSampleControlButton (NativeHandle handle) : base (new MockSampleControl (), handle)
		{
		}

		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public MockedSampleControlButton (NSCoder coder) : base (new MockSampleControl (), coder)
		{
		}
	}
}
