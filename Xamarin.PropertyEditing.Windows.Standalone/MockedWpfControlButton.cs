using Xamarin.PropertyEditing.Tests.MockControls;

namespace Xamarin.PropertyEditing.Windows.Standalone
{

	public class MockedWpfButton: MockedControlButton<MockWpfButton> {
		public MockedWpfButton() : base(new MockWpfButton()) { }
	}
}
