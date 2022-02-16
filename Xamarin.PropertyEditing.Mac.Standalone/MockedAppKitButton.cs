using System;
using Foundation;
using ObjCRuntime;
using Xamarin.PropertyEditing.Tests.MockControls;

namespace Xamarin.PropertyEditing.Mac.Standalone
{
	[Foundation.Register ("MockedAppKitButton")]
	public class MockedAppKitButton : MockedControlButton<MockNSButton>
	{
		// Called when created from unmanaged code
		public MockedAppKitButton (NativeHandle handle) : base (new MockNSButton (), handle)
		{
		}

		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public MockedAppKitButton (NSCoder coder) : base (new MockNSButton (), coder)
		{
		}
	}
}
