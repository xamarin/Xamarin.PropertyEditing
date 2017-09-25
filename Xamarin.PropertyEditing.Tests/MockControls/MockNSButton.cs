namespace Xamarin.PropertyEditing.Tests.MockControls
{
	public class MockNSButton : MockNSControl
	{
		public MockNSButton ()
		{
			AddProperty<bool> ("AllowsMixedState");
			AddProperty<NotImplemented> ("AlternateImage");
			AddProperty<string> ("AlternateTitle");
			AddProperty<NotImplemented> ("AttributedAlternateTitle");
			AddProperty<NotImplemented> ("AttributedTitle");
			AddProperty<NSBezelStyle> ("BezelStyle");
			AddProperty<bool> ("Bordered");
			AddProperty<NotImplemented> ("Cell");
			AddReadOnlyProperty<NotImplemented> ("ClassHandle");
			AddProperty<NotImplemented> ("Image");
			AddProperty<NSCellImagePosition> ("ImagePosition");
			AddProperty<string> ("KeyEquivalent");
			AddProperty<NSEventModifierMask> ("KeyEquivalentModifierMask");
			AddProperty<NotImplemented> ("Sound");
			AddProperty<NSCellStateValue> ("State");
			AddProperty<string> ("Title");
			AddProperty<bool> ("Transparent");
		}

		public enum NSBezelStyle
		{
			Circular,
			Disclosure,
			HelpButton,
			Inline,
			Recessed,
			RegularSquare,
			Rounded,
			RoundedDisclosure,
			RoundRect,
			ShadowlessSquare,
			SmallSquare,
			TexturedRounded,
			TexturedSquare,
			ThickerSquare,
			ThickSquare
		}

		public enum NSCellImagePosition
		{
			ImageAbove,
			ImageBelow,
			ImageLeft,
			ImageOnly,
			ImageOverlaps,
			ImageRight,
			NoImage,
		}

		public enum NSEventModifierMask
		{
			AlphaShiftKeyMask,
			AlternateKeyMask,
			CommandKeyMask,
			ControlKeyMask,
			DeviceIndependentModifierFlagsMask,
			FunctionKeyMask,
			HelpKeyMask,
			NumericPadKeyMask,
			ShiftKeyMask
		}

		public enum NSCellStateValue
		{
			Mixed,
			Off,
			On
		}
	}
}
