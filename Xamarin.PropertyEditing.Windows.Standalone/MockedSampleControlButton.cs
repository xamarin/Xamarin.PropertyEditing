using System.Drawing;
using Xamarin.PropertyEditing.Drawing;
using Xamarin.PropertyEditing.Tests.MockControls;

namespace Xamarin.PropertyEditing.Windows.Standalone
{
	public class MockedSampleControlButton : MockedControlButton<MockSampleControl>
	{
		public MockedSampleControlButton () : base (new MockSampleControl ())
		{
			// TODO: Move the declaration of this property to MockSampleControl once SolidBrush is supported on both platforms.
			MockedControl.AddProperty<CommonSolidBrush> ("SolidBrush", "Windows Only");
			MockedControl.SetValue("SolidBrush",
				new CommonSolidBrush(20, 120, 220, 240));
		}
	}
}
