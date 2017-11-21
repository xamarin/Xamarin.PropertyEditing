using System.Threading.Tasks;
using Xamarin.PropertyEditing.Drawing;
using Xamarin.PropertyEditing.Tests.MockControls;

namespace Xamarin.PropertyEditing.Windows.Standalone
{
	public class MockedSampleControlButton : MockedControlButton<MockSampleControl>
	{
		public MockedSampleControlButton () : base (new MockSampleControl ())
		{
			// TODO: Move the declaration of this property to MockSampleControl once SolidBrush is supported on both platforms.
			this.brushPropertyInfo = new BrushPropertyInfo (
				name: "SolidBrush",
				category: "Windows Only",
				canWrite: true,
				colorSpaces: new[] { "RGB", "sRGB" });
			MockedControl.AddProperty<CommonBrush> (this.brushPropertyInfo);

			this.readOnlyBrushPropertyInfo = new BrushPropertyInfo (
				name: "ReadOnlySolidBrush",
				category: "Windows Only",
				canWrite: false);
			MockedControl.AddProperty<CommonBrush> (this.readOnlyBrushPropertyInfo);
		}

		public async Task SetBrush (IObjectEditor editor, CommonBrush brush)
		{
			await editor.SetValueAsync (this.brushPropertyInfo, new ValueInfo<CommonBrush> { Value = brush });
		}

		public async Task SetReadOnlyBrush (IObjectEditor editor, CommonBrush brush)
		{
			await editor.SetValueAsync (this.readOnlyBrushPropertyInfo, new ValueInfo<CommonBrush> { Value = brush });
		}

		private IPropertyInfo brushPropertyInfo;
		private IPropertyInfo readOnlyBrushPropertyInfo;
	}
}
