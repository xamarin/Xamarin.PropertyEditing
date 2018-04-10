using System;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.Tests.MockControls
{
	public class MockSampleControl : MockControl
	{
		public MockSampleControl()
		{
			AddProperty<bool> ("Boolean", ReadWrite);
			AddProperty<bool> ("UnsetBoolean", ReadWrite, valueSources: ValueSources.Local);
			AddProperty<int> ("Integer", ReadWrite);
			AddProperty<int> ("UnsetInteger", ReadWrite, valueSources: ValueSources.Local);
			AddProperty<float> ("FloatingPoint", ReadWrite);
			AddProperty<string> ("String", ReadWrite);
			AddProperty<Enumeration> ("Enumeration", ReadWrite);
			AddProperty<FlagsNoValues> ("FlagsNoValues", ReadWrite, canWrite: true, flag: true);
			AddProperty<FlagsWithValues> ("FlagsWithValues", ReadWrite, canWrite: true, flag: true);
			AddProperty<CommonPoint> ("Point", ReadWrite);
			AddProperty<CommonSize> ("Size", ReadWrite);
			AddProperty<CommonRectangle> ("Rectangle", ReadWrite);
			// AddProperty<CommonThickness> ("Thickness", ReadWrite); // Lacking support on the mac at this point in time.

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
			// AddReadOnlyProperty<CommonThickness> ("ReadOnlyThickness", ReadOnly);

			AddProperty<NotImplemented> ("Uncategorized", None);

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
	}
}
