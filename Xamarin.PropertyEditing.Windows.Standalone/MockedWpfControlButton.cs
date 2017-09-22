using System.Windows.Controls;
using Xamarin.PropertyEditing.Tests.MockControls;

namespace Xamarin.PropertyEditing.Windows.Standalone
{
	public abstract class MockedWpfControlButton<T> : Button
		where T : MockControl
	{
		public MockedWpfControlButton(T mockedControl)
		{
			MockedControl = mockedControl;
		}

		public T MockedControl { get; }
	}

	public class MockedWpfButton: MockedWpfControlButton<MockWpfButton> {
		public MockedWpfButton() : base(new MockWpfButton()) { }
	}
}
