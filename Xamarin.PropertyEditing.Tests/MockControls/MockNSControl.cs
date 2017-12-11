using System;
using Xamarin.PropertyEditing.Drawing;
using static Xamarin.PropertyEditing.Tests.MockPropertyInfo.MockPropertyCategories;

namespace Xamarin.PropertyEditing.Tests.MockControls
{
	public class MockNSControl : MockControl
	{
		public MockNSControl ()
		{
			#region NSView Properties
			AddProperty<bool> ("AcceptsTouchEvents");
			AddReadOnlyProperty<NotImplemented> ("AlignmentRectInsets");
			AddProperty<double> ("Alphavalue"); // TODO: make that float.
			AddProperty<NotImplemented> ("Animations");
			AddReadOnlyProperty<NotImplemented> ("Animator");
			AddProperty<bool> ("AutoresizesSubviews");
			AddProperty<NSViewResizingMask> ("AutoResizingMask", None, true, true);
			AddProperty<NotImplemented> ("BackgroundFilters");
			AddReadOnlyProperty<double> ("BaselineOffsetFromBottom"); //TODO: make that float
			AddProperty<CommonRectangle> ("Bounds"); // TODO: disentangle the rectangle editor from System.Drawing.Rectangle (or use that type for the mac mocks).
			AddProperty<double> ("BoundsRotation");
			AddReadOnlyProperty<bool> ("CanBecomeKeyView");
			AddProperty<bool> ("CanDrawConcurrently");
			AddProperty<NotImplemented> ("CompositingFilter");
			AddReadOnlyProperty<NotImplemented> ("Constraints");
			AddProperty<NotImplemented> ("ContentFilters");
			AddReadOnlyProperty<NotImplemented> ("EnclosingScrollView");
			AddReadOnlyProperty<CommonSize> ("FittingSize"); // TODO: disentangle the size editor from System.Drawing.Size
			AddReadOnlyProperty<CommonRectangle> ("FocusRingMaskBounds");
			AddProperty<NSFocusRingType> ("FocusRingType");
			AddProperty<CommonRectangle> ("Frame");
			AddProperty<double> ("FrameCenterRotation"); // TODO: float
			AddProperty<double> ("FrameRotation"); // TODO: float
			AddReadOnlyProperty<bool> ("HasAmbiguousLayout");
			AddReadOnlyProperty<double> ("HeightAdjustLimit"); // TODO: float
			AddProperty<bool> ("Hidden");
			AddProperty<string> ("Identifier");
			AddReadOnlyProperty<bool> ("InLiveResize");
			AddReadOnlyProperty<NotImplemented> ("InputContext");
			AddReadOnlyProperty<CommonSize> ("IntrinsicContentSize");
			AddReadOnlyProperty<bool> ("IsFlipped");
			AddReadOnlyProperty<bool> ("IsHiddenOrHasHiddenAncestor");
			AddReadOnlyProperty<bool> ("IsInFullScreenMode");
			AddReadOnlyProperty<bool> ("IsOpaque");
			AddReadOnlyProperty<bool> ("IsRotatedFromBase");
			AddReadOnlyProperty<bool> ("IsRotatedOrScaledFromBase");
			AddProperty<NotImplemented> ("Layer");
			// TODO: finish that and add NSResponder properties, etc.
			#endregion

			#region NSControl Properties
			AddProperty<NotImplemented> ("Action");
			AddProperty<NSTextAlignment> ("Alignment");
			AddProperty<NotImplemented> ("AttributedStringValue");
			AddProperty<NSWritingDirection> ("BaseWritingDirection");
			AddProperty<bool> ("Continuous");
			AddProperty<double> ("DoubleValue");
			AddProperty<bool> ("Enabled");
			AddProperty<double> ("FloatValue"); // TODO: fix float. Currently, one must use double instead.
			AddProperty<NotImplemented> ("Font");
			AddProperty<NotImplemented> ("Formatter");
			AddProperty<bool> ("IgnoresMultiClick");
			AddProperty<long> ("IntValue");
			AddProperty<NotImplemented> ("ObjectValue");
			AddProperty<bool> ("RefusesFirstResponder");
			AddReadOnlyProperty<NotImplemented> ("SelectedCell");
			AddReadOnlyProperty<long> ("SelectedTag");
			AddProperty<string> ("StringValue");
			AddProperty<long> ("Tag");
			AddProperty<NotImplemented> ("Target");
			#endregion

			#region Events
			AddEvents ("Activated");
			#endregion
		}

		public enum NSTextAlignment
		{
			Center,
			Justified,
			Left,
			Natural,
			Right
		}

		public enum NSWritingDirection
		{
			Embedding,
			LeftToRight,
			Natural,
			Override,
			RightToLeft
		}

		[Flags]
		public enum NSViewResizingMask
		{
			HeightSizable,
			MaxXMargin,
			MaxYMargin,
			MinXMargin,
			MinYMargin,
			NotSizable,
			WidthSizable
		}

		public enum NSFocusRingType
		{
			Default,
			Exterior,
			None
		}
	}
}
