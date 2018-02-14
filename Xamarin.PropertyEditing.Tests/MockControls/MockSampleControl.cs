using System;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.Tests.MockControls
{
	public class MockSampleControl : MockControl
	{
		public MockSampleControl()
		{
			AddProperty<bool> ("Boolean", ReadWrite);
			AddProperty<int> ("Integer", ReadWrite);
			AddProperty<float> ("FloatingPoint", ReadWrite);
			AddProperty<string> ("String", ReadWrite);
			AddProperty<Enumeration> ("Enumeration", ReadWrite);
			AddProperty<Flags> ("Flags", ReadWrite, canWrite: true, flag: true);
			AddProperty<CommonPoint> ("Point", ReadWrite);
			AddProperty<CommonSize> ("Size", ReadWrite);
			AddProperty<CommonRectangle> ("Rectangle", ReadWrite);
			// AddProperty<CommonThickness> ("Thickness", ReadWrite); // Lacking support on the mac at this point in time.

			AddReadOnlyProperty<bool> ("ReadOnlyBoolean", ReadOnly);
			AddReadOnlyProperty<int> ("ReadOnlyInteger", ReadOnly);
			AddReadOnlyProperty<float> ("ReadOnlyFloatingPoint", ReadOnly);
			AddReadOnlyProperty<string> ("ReadOnlyString", ReadOnly);
			AddReadOnlyProperty<Enumeration> ("ReadOnlyEnumeration", ReadOnly);
			AddProperty<Flags> ("ReadOnlyFlags", ReadOnly, canWrite: false, flag: true);
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

		[Flags]
		public enum Flags
		{
			FlagOne,
			FlagTwo,
			FlagThree
		}
	}
}
