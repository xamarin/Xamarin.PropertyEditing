using Xamarin.PropertyEditing.Drawing;
using static Xamarin.PropertyEditing.Tests.MockPropertyInfo.MockPropertyCategories;

namespace Xamarin.PropertyEditing.Tests.MockControls
{
	public class MockWpfControl : MockControl
	{
		public MockWpfControl ()
		{
			#region Properties
			// TODO: handle the currently unsupported property types
			// Note: properties marked as obsolete are omitted.
			AddProperty<double> ("ActualHeight", Layout, false);
			AddProperty<double> ("ActualWidth", Layout, false);
			AddProperty<bool> ("AllowDrop");
			AddProperty<bool> ("AreAnyTouchesCaptured", None, false);
			AddProperty<bool> ("AreAnyTouchesCapturedWithin", None, false);
			AddProperty<bool> ("AreAnyTouchesDirectlyOver", None, false);
			AddProperty<bool> ("AreAnyTouchesOver", None, false);
			AddProperty<CommonBrush> ("BackGround", Appearance);
			AddProperty<NotImplemented> ("BindingGroup");
			AddProperty<NotImplemented> ("BitmapEffect");
			AddProperty<NotImplemented> ("BitmapEffectInput");
			AddProperty<CommonBrush> ("BorderBrush", Appearance);
			AddProperty<CommonThickness> ("BorderThickness", Appearance);
			AddProperty<NotImplemented> ("CacheMode");
			AddProperty<NotImplemented> ("Clip");
			AddProperty<bool> ("ClipToBounds");
			AddProperty<NotImplemented> ("CommandBindings", None, false);
			AddProperty<NotImplemented> ("ContextMenu");
			AddProperty<NotImplemented> ("Cursor");
			AddProperty<object> ("DataContext");
			AddProperty<NotImplemented> ("DependencyObjectType", None, false);
			AddProperty<CommonSize> ("DesiredSize", None, false);
			AddProperty<NotImplemented> ("Dispatcher", None, false);
			AddProperty<NotImplemented> ("Effect");
			AddProperty<FlowDirection> ("FlowDirection");
			AddProperty<bool> ("Focusable");
			AddProperty<NotImplemented> ("FocusVisualStyle");
			AddProperty<NotImplemented> ("FontFamily", Appearance);
			AddProperty<double> ("FontSize", Appearance);
			AddProperty<NotImplemented> ("FontStretch", Appearance);
			AddProperty<NotImplemented> ("FontStyle", Appearance);
			AddProperty<NotImplemented> ("FontWeight", Appearance);
			AddProperty<bool> ("ForceCursor");
			AddProperty<CommonBrush> ("Foreground", Appearance);
			AddProperty<bool> ("HasAnimatedProperties", None, false);
			AddProperty<double> ("Height");
			AddProperty<HorizontalAlignment> ("HorizontalAlignment");
			AddProperty<HorizontalAlignment> ("HorizontalContentAlignment", Layout);
			AddProperty<NotImplemented> ("InputBindings", None, false);
			AddProperty<NotImplemented> ("InputScope");
			AddProperty<bool> ("IsArrangeValid", None, false);
			AddProperty<bool> ("IsEnabled");
			AddProperty<bool> ("IsFocused", None, false);
			AddProperty<bool> ("IsHitTestVisible");
			AddProperty<bool> ("IsInitialized", None, false);
			AddProperty<bool> ("IsInputMethodEnabled", None, false);
			AddProperty<bool> ("IsKeyboardFocused", None, false);
			AddProperty<bool> ("IsKeyboardFocusWithin", None, false);
			AddProperty<bool> ("IsLoaded", None, false);
			AddProperty<bool> ("IsManipulationEnabled", Touch);
			AddProperty<bool> ("IsMeasureValid", None, false);
			AddProperty<bool> ("IsMouseCaptured", None, false);
			AddProperty<bool> ("IsMouseCaptureWithin", None, false);
			AddProperty<bool> ("IsMouseDirectlyOver", None, false);
			AddProperty<bool> ("IsMouseOver", None, false);
			AddProperty<bool> ("IsSealed", None, false);
			AddProperty<bool> ("IsStylusCaptured", None, false);
			AddProperty<bool> ("IsStylusCaptureWithin", None, false);
			AddProperty<bool> ("IsStylusDirectlyOver", None, false);
			AddProperty<bool> ("IsStylusOver", None, false);
			AddProperty<bool> ("IsTabStop", Behavior);
			AddProperty<bool> ("IsVisible", None, false);
			AddProperty<NotImplemented> ("Language");
			AddProperty<NotImplemented> ("LayoutTranform");
			AddProperty<CommonThickness> ("Margin");
			AddProperty<double> ("MaxHeight");
			AddProperty<double> ("MaxWidth");
			AddProperty<string> ("Name");
			AddProperty<double> ("Opacity");
			AddProperty<CommonBrush> ("OpacityMask");
			AddProperty<bool> ("OverridesDefaultStyle");
			AddProperty<CommonThickness> ("Padding", Layout);
			AddProperty<NotImplemented> ("Parent", None, false);
			AddProperty<CommonSize> ("RenderSize");
			AddProperty<NotImplemented> ("RenderTransform");
			AddProperty<CommonPoint> ("RenderTransformOrigin");
			AddProperty<NotImplemented> ("Resources");
			AddProperty<bool> ("SnapsToDevicePixels");
			AddProperty<NotImplemented> ("Style");
			AddProperty<long> ("TabIndex", Behavior); // TODO: fix int. Won't work for now. Use long.
			AddProperty<object> ("Tag");
			AddProperty<NotImplemented> ("Template");
			AddProperty<NotImplemented> ("TemplatedParent");
			AddProperty<string> ("ToolTip", Appearance);
			AddProperty<NotImplemented> ("TouchesCaptured", None, false);
			AddProperty<NotImplemented> ("TouchesCapturedWithin", None, false);
			AddProperty<NotImplemented> ("TouchesDirectlyOver", None, false);
			AddProperty<NotImplemented> ("TouchesOver", None, false);
			AddProperty<NotImplemented> ("Triggers", None, false);
			AddProperty<string> ("Uid");
			AddProperty<bool> ("UseLayoutRounding");
			AddProperty<VerticalAlignment> ("VerticalAlignment");
			AddProperty<VerticalAlignment> ("VerticalContentAlignment", Layout);
			AddProperty<Visibility> ("Visibility");
			AddProperty<double> ("Width");
			AddProperty<CommonRatio> ("Ratio");
			#endregion
			#region Events
			AddEvents (
				"ContextMenuClosing",
				"ContextMenuOpening",
				"DataContextChanged",
				"DragEnter",
				"DragLeave",
				"DragOver",
				"Drop",
				"FocusableChanged",
				"GiveFeedback",
				"GotFocus",
				"GotKeyboardFocus",
				"GotMouseCapture",
				"GotStylusCapture",
				"GotTouchCapture",
				"Initialized",
				"IsEnabledChanged",
				"IsHitTestVisibleChanged",
				"IsKeyboardFocusedChanged",
				"IsKeyboardFocusWithinChanged",
				"IsMouseCapturedChanged",
				"IsMouseCaptureWithinChanged",
				"IsMouseDirectlyOverChanged",
				"IsStylusCapturedChanged",
				"IsStylusCaptureWithinChanged",
				"IsStylusDirectlyOverChanged",
				"IsVisibleChanged",
				"KeyDown",
				"KeyUp",
				"LayoutUpdated",
				"Loaded",
				"LostFocus",
				"LostKeyboardFocus",
				"LostMouseCapture",
				"LostStylusCapture",
				"LostTouchCapture",
				"ManipulationBoundaryFeedback",
				"ManipulationCompleted",
				"ManipulationDelta",
				"ManipulationInertiaStarting",
				"ManipulationStarted",
				"ManipulationStarting",
				"MouseDoubleClick",
				"MouseDown",
				"MouseEnter",
				"MouseLeave",
				"MouseLeftButtonDown",
				"MouseLeftButtonUp",
				"MouseMove",
				"MouseRightButtonDown",
				"MouseRightButtonUp",
				"MouseUp",
				"MouseWheel",
				"PreviewDragEnter",
				"PreviewDragLeave",
				"PreviewDragOver",
				"PreviewDrop",
				"PreviewGiveFeedback",
				"PreviewGotKeyboardFocus",
				"PreviewKeyDown",
				"PreviewKeyUp",
				"PreviewLostKeyboardFocus",
				"PreviewMouseDoubleClick",
				"PreviewMouseDown",
				"PreviewMouseLeftButtonDown",
				"PreviewMouseLeftButtonUp",
				"PreviewMouseMove",
				"PreviewMouseRightButtonDown",
				"PreviewMouseRightButtonUp",
				"PreviewMouseUp",
				"PreviewMouseWheel",
				"PreviewQueryContinueDrag",
				"PreviewStylusButtonDown",
				"PreviewStylusButtonUp",
				"PreviewStylusDown",
				"PreviewStylusInAirMove",
				"PreviewStylusInRange",
				"PreviewStylusMove",
				"PreviewStylusOutOfRange",
				"PreviewStylusSystemGesture",
				"PreviewStylusUp",
				"PreviewTextInput",
				"PreviewTouchDown",
				"PreviewTouchMove",
				"PreviewTouchUp",
				"QueryContinueDrag",
				"QueryCursor",
				"RequestBringIntoView",
				"SizeChanged",
				"SourceUpdated",
				"StylusButtonDown",
				"StylusButtonUp",
				"StylusDown",
				"StylusEnter",
				"StylusInAirMove",
				"StylusInRange",
				"StylusLeave",
				"StylusMove",
				"StylusOutOfRange",
				"StylusSystemGesture",
				"StylusUp",
				"TargetUpdated",
				"TextInput",
				"ToolTipClosing",
				"ToolTipOpening",
				"TouchDown",
				"TouchEnter",
				"TouchLeave",
				"TouchMove",
				"TouchUp",
				"Unloaded"
				);
			#endregion
		}

		public enum FlowDirection
		{
			LeftToRight,
			RightToLeft
		}

		public enum HorizontalAlignment
		{
			Center,
			Left,
			Right,
			Stretch
		}

		public enum VerticalAlignment
		{
			Bottom,
			Center,
			Stretch,
			Top
		}

		public enum Visibility
		{
			Collapsed,
			Hidden,
			Visible
		}
	}
}
