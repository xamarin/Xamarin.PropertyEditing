using System;
using Foundation;
using AppKit;
using Xamarin.PropertyEditing.Tests.MockControls;

namespace Xamarin.PropertyEditing.Mac.Standalone
{
	public abstract class MockedControlButton<T> : NSButtonCell, IMockedControl
		where T : MockControl
	{
		protected MockedControlButton (T mockedControl, IntPtr handle) : base (handle)
		{
			MockedControl = mockedControl;
			Initialize ();
		}

		protected MockedControlButton (T mockedControl, NSCoder coder) : base (coder)
		{
			Initialize ();
			MockedControl = mockedControl;
		}

		// Shared initialization code
		void Initialize ()
		{
		}

		public T MockedControl { get; }

		object IMockedControl.MockedControl => MockedControl;
	}
}
