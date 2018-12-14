using System;
using System.Collections;
using System.Threading.Tasks;
using Xamarin.PropertyEditing.Drawing;
using Xamarin.PropertyEditing.Tests.MockPropertyInfo;

namespace Xamarin.PropertyEditing.Tests.MockControls
{
	public class MockSampleControl : MockControl
	{
		public MockSampleControl ()
		{
			AddProperty<char> ("Char", ReadWrite, valueSources: ValueSources.Local | ValueSources.Resource | ValueSources.Binding);
			AddProperty<DateTime> ("DateTime", ReadWrite, valueSources: ValueSources.Local | ValueSources.Resource | ValueSources.Binding);
			AddProperty<TimeSpan> ("TimeSpan", ReadWrite, valueSources: ValueSources.Local | ValueSources.Resource | ValueSources.Binding);
			AddProperty<bool> ("Boolean", ReadWrite, valueSources: ValueSources.Local | ValueSources.Resource | ValueSources.Binding);
			AddProperty<FilePath> ("FilePath", ReadWrite, valueSources: ValueSources.Local | ValueSources.Resource | ValueSources.Binding);
			AddProperty<bool> ("UnsetBoolean", ReadWrite, valueSources: ValueSources.Local);
			AddProperty<DirectoryPath> ("DirectoryPath", ReadWrite, valueSources: ValueSources.Local | ValueSources.Resource | ValueSources.Binding);
			AddProperty<int> ("Integer", ReadWrite);
			AddProperty<int> ("UnsetInteger", ReadWrite, valueSources: ValueSources.Local);
			AddProperty<float> ("FloatingPoint", ReadWrite);
			AddProperty<string> ("String", ReadWrite, valueSources: ValueSources.Local | ValueSources.Resource | ValueSources.Binding);
			AddProperty<int> ("Width", ReadWrite, valueSources: ValueSources.Local | ValueSources.Resource | ValueSources.Binding, inputModes: new[] { new InputMode("Auto", true), new InputMode("Star"), new InputMode("Pixel"), });
			AddProperty<Enumeration> ("Enumeration", ReadWrite, constrained: false);
			AddProperty<string> ("StringV", ReadWrite, valueSources: ValueSources.Local | ValueSources.Resource | ValueSources.Binding,
				options: new [] {
					new PropertyVariationOption ("Width", "Compact"),
					new PropertyVariationOption ("Width", "Regular"),
					new PropertyVariationOption ("Gamut", "P3"),
					new PropertyVariationOption ("Gamut", "sRGB"),
					new PropertyVariationOption ("OnPlatform", "iOS"),
					new PropertyVariationOption ("OnPlatform", "Android"),
					new PropertyVariationOption ("OnPlatform", "UWP"),
				});
			AddProperty<FlagsNoValues> ("FlagsNoValues", ReadWrite, canWrite: true, flag: true);
			AddProperty<FlagsWithValues> ("FlagsWithValues", ReadWrite, canWrite: true, flag: true);
			AddProperty<CommonPoint> ("Point", ReadWrite, isUncommon: true);
			AddProperty<CommonSize> ("Size", ReadWrite, isUncommon: true);
			AddProperty<CommonRectangle> ("Rectangle", ReadWrite);
			AddProperty<CommonRatio> ("Ratio", ReadWrite);
			AddProperty<CommonThickness> ("Thickness", ReadWrite);
			AddProperty<object> ("Object", ReadWrite);
			AddProperty<IList> ("Collection", ReadWrite);

			AddReadOnlyProperty<bool> ("ReadOnlyBoolean", ReadOnly);
			AddReadOnlyProperty<int> ("ReadOnlyInteger", ReadOnly);
			AddReadOnlyProperty<float> ("ReadOnlyFloatingPoint", ReadOnly);
			AddReadOnlyProperty<string> ("ReadOnlyString", ReadOnly);
			AddReadOnlyProperty<Enumeration> ("ReadOnlyEnumeration", ReadOnly);
			AddProperty<FlagsNoValues> ("ReadOnlyFlagsNotValue", ReadOnly, canWrite: false, flag: true);
			AddProperty<FlagsWithValues> ("ReadOnlyFlagsWithValues", ReadOnly, canWrite: false, flag: true);
			AddReadOnlyProperty<CommonPoint> ("ReadOnlyPoint", ReadOnly);
			AddReadOnlyProperty<CommonSize> ("ReadOnlySize", ReadOnly);
			AddReadOnlyProperty<CommonRectangle> ("ReadOnlyRectangle", ReadOnly);
			AddReadOnlyProperty<CommonRatio> ("ReadOnlyRatio", ReadOnly);
			AddReadOnlyProperty<CommonThickness> ("ReadOnlyThickness", ReadOnly);

			AddProperty<NotImplemented> ("Uncategorized", None);
			AddProperty<string> ("ReadOnlyStringWithInputMode", ReadOnly, canWrite: false, flag: true, valueSources: ValueSources.Local | ValueSources.Resource | ValueSources.Binding, inputModes: new[] { new InputMode ("Auto", true), new InputMode ("Star"), new InputMode ("Pixel"), });
			AddProperty<string> ("StringWithInputMode", ReadWrite, valueSources: ValueSources.Local | ValueSources.Resource | ValueSources.Binding, inputModes: new[] { new InputMode ("Auto", true), new InputMode ("Star"), new InputMode ("Pixel"), });

			// TODO: Move the declaration of this property to MockSampleControl once SolidBrush is supported on both platforms.
			this.brushPropertyInfo = new MockBrushPropertyInfo (
				name: "SolidBrush",
				category: null,
				canWrite: true,
				colorSpaces: new[] { "RGB", "sRGB" });
			AddProperty<CommonBrush> (this.brushPropertyInfo);

			this.materialDesignBrushPropertyInfo = new MockBrushPropertyInfo (
				name: "MaterialDesignBrush",
				category: null,
				canWrite: true);
			AddProperty<CommonBrush> (this.materialDesignBrushPropertyInfo);

			this.readOnlyBrushPropertyInfo = new MockBrushPropertyInfo (
				name: "ReadOnlySolidBrush",
				category: null,
				canWrite: false);
			AddProperty<CommonBrush> (this.readOnlyBrushPropertyInfo);

			this.colorPropertyInfo = new MockPropertyInfo<CommonColor> (
				name: "ColorNoBrush",
				category: "Windows Only",
				canWrite: true,
				valueSources: ValueSources.Default | ValueSources.Local | ValueSources.Resource);
			AddProperty<CommonColor> (this.colorPropertyInfo);

			AddEvents ("Click", "Hover", "Focus");

		}

		// Categories
		public static readonly string ReadWrite = nameof (ReadWrite);
		public static readonly string ReadOnly = nameof (ReadOnly);
		public static readonly string None = "";

		// Some enumeration and flags to test with
		public enum Enumeration
		{
			FirstOption,
			SecondOption,
			ThirdOption
		}

		[Flags] // Treated like a 0 based Enum
		public enum FlagsNoValues
		{
			FlagNoValueZero,
			FlagNoValueOne,
			FlagNoValueTwo,
		}

		[Flags]
		public enum FlagsWithValues
		{
			FlagHasValueOne = 1,
			FlagHasValueTwo = 2,
			FlagHasValueFour = 4,
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
		private IPropertyInfo colorPropertyInfo;
		private bool brushSet = false;
		private bool materialDesignBrushSet = false;
		private bool readOnlyBrushSet = false;
	}
}
