using System.Windows.Controls;
using Xamarin.PropertyEditing.Tests.MockControls;

namespace Xamarin.PropertyEditing.Windows.Standalone
{
	public abstract class MockedControlButton<T> : Button, IMockedControl
		where T : MockControl
	{
		public MockedControlButton(T mockedControl)
		{
			MockedControl = mockedControl;
		}

		public T MockedControl { get; }

		object IMockedControl.MockedControl => MockedControl;
	}
}
