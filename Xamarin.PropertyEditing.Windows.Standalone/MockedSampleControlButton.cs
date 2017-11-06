using Xamarin.PropertyEditing.Drawing;
using Xamarin.PropertyEditing.Tests.MockControls;

namespace Xamarin.PropertyEditing.Windows.Standalone
{
	public class MockedSampleControlButton : MockedControlButton<MockSampleControl>
	{
		public MockedSampleControlButton () : base (new MockSampleControl ())
		{
			// TODO: Move the declaration of this property to MockSampleControl once SolidBrush is supported on both platforms.
			var brushPropertyInfo = new BrushPropertyInfo (
				name: "SolidBrush",
				category: "Windows Only",
				canWrite: true,
				colorSpaces: new[] { "RGB", "sRGB" });
			MockedControl.AddProperty<CommonBrush> (brushPropertyInfo);
			MockedControl.SetValue<CommonBrush>(brushPropertyInfo,
				new CommonSolidBrush(20, 120, 220, 240, "sRGB"));

			var readOnlyBrushPropertyInfo = new BrushPropertyInfo(
				name: "ReadOnlySolidBrush",
				category: "Windows Only",
				canWrite: false);
			MockedControl.AddProperty<CommonBrush> (readOnlyBrushPropertyInfo);
			MockedControl.SetValue<CommonBrush> (readOnlyBrushPropertyInfo,
				new CommonSolidBrush (240, 220, 15, 190));
		}
	}
}
