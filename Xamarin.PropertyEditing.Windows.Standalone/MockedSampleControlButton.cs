using Xamarin.PropertyEditing.Tests.MockControls;

namespace Xamarin.PropertyEditing.Windows.Standalone
{
	public class MockedSampleControlButton: MockedControlButton<MockSampleControl> {
		public MockedSampleControlButton () : base(new MockSampleControl ()) { }
	}
}
