using System;
using Foundation;
using AppKit;
using Xamarin.PropertyEditing.Tests.MockControls;

namespace Xamarin.PropertyEditing.Mac.Standalone
{
	public abstract class MockedAppKitControlButton<T> : AppKit.NSButtonCell where T : MockControl
	{
		protected MockedAppKitControlButton (T mockedControl, IntPtr handle) : base (handle)
		{
			MockedControl = mockedControl;
			Initialize ();
		}

		protected MockedAppKitControlButton (T mockedControl, NSCoder coder) : base (coder)
		{
			Initialize ();
			MockedControl = mockedControl;
		}

		// Shared initialization code
		void Initialize ()
		{
		}

		public T MockedControl { get; }
	}

	public partial class MockedAppKitButton : MockedAppKitControlButton<MockNSButton>
	{
		// Called when created from unmanaged code
		public MockedAppKitButton (IntPtr handle) : base (new MockNSButton (), handle)
		{
		}

		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public MockedAppKitButton (NSCoder coder) : base (new MockNSButton (), coder)
		{
		}
	}
}
