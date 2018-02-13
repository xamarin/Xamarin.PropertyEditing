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

			this.materialDesignBrushPropertyInfo = new BrushPropertyInfo (
				name: "MaterialDesignBrush",
				category: "Windows Only",
				canWrite: true);
			MockedControl.AddProperty<CommonBrush> (this.materialDesignBrushPropertyInfo);

			this.readOnlyBrushPropertyInfo = new BrushPropertyInfo (
				name: "ReadOnlySolidBrush",
				category: "Windows Only",
				canWrite: false);
			MockedControl.AddProperty<CommonBrush> (this.readOnlyBrushPropertyInfo);
		}

		public async Task SetBrushInitialValueAsync (IObjectEditor editor, CommonBrush brush)
		{
			if (this.brushSet) return;
			await editor.SetValueAsync (this.brushPropertyInfo, new ValueInfo<CommonBrush> { Value = brush });
			this.brushSet = true;
		}

		public async Task SetMaterialDesignBrushInitialValueAsync (IObjectEditor editor, CommonBrush brush)
		{
			if (this.materialDesignBrushSet) return;
			await editor.SetValueAsync (this.materialDesignBrushPropertyInfo, new ValueInfo<CommonBrush> { Value = brush });
			this.materialDesignBrushSet = true;
		}

		public async Task SetReadOnlyBrushInitialValueAsync (IObjectEditor editor, CommonBrush brush)
		{
			if (this.readOnlyBrushSet) return;
			await editor.SetValueAsync (this.readOnlyBrushPropertyInfo, new ValueInfo<CommonBrush> { Value = brush });
			this.readOnlyBrushSet = true;
		}

		private IPropertyInfo brushPropertyInfo;
		private IPropertyInfo materialDesignBrushPropertyInfo;
		private IPropertyInfo readOnlyBrushPropertyInfo;
		private bool brushSet = false;
		private bool materialDesignBrushSet = false;
		private bool readOnlyBrushSet = false;
	}
}
