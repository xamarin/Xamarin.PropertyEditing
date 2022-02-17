using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AppKit;
using Foundation;
using ObjCRuntime;
using Xamarin.PropertyEditing.Drawing;
using Xamarin.PropertyEditing.Tests;

namespace Xamarin.PropertyEditing.Mac.Standalone
{
	public partial class ViewController : NSViewController
	{
		public ViewController (NativeHandle handle) : base (handle)
		{
			
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			// Do any additional setup after loading the view.
			var resourceProvider = new MockResourceProvider ();
			PropertyPanel.TargetPlatform = new TargetPlatform (new MockEditorProvider (resourceProvider), resourceProvider, new MockBindingProvider()) {
				SupportsCustomExpressions = true,
				SupportsMaterialDesign = true,
				SupportsBrushOpacity = false,
				GroupedTypes = new Dictionary<Type, string> {
					{ typeof(CommonBrush), "Brush" }
				},
				AutoExpandGroups = new [] { "ReadWrite" }
			};
		}

		public override NSObject RepresentedObject {
			get {
				return base.RepresentedObject;
			}
			set {
				base.RepresentedObject = value;
				// Update the view, if already loaded.
			}
		}

		private bool addToSelection = true;

		// load panel from active designer item, clear it if none selected
		partial void OnClickEvent (NSObject sender)
		{
			var clickedButton = sender as NSButton;
			var mockedButton = clickedButton?.Cell as IMockedControl;

			SetInitialValuesAsync (mockedButton as MockedSampleControlButton).Wait ();

			var inspectedObject = (mockedButton == null || mockedButton.MockedControl == null)
				? (object)sender : mockedButton.MockedControl;

			if (this.addToSelection) {
				if (PropertyPanel.SelectedItems.Contains (inspectedObject)) {
					PropertyPanel.SelectedItems.Remove (inspectedObject);
				} else {
					PropertyPanel.SelectedItems.Add (inspectedObject);
				}
			} else {
				PropertyPanel.Select (new[] { inspectedObject });
			}
		}

		partial void OnSelectionModeChanged (NSObject sender)
		{
			this.addToSelection = ((NSButton)sender).State == NSCellStateValue.On;
		}

		async Task SetInitialValuesAsync (MockedSampleControlButton mocked)
		{
			if (mocked == null)
				return;

			IObjectEditor editor = await PropertyPanel.TargetPlatform.EditorProvider.GetObjectEditorAsync (mocked.MockedControl);
			await mocked.MockedControl.SetInitialValuesAsync (editor);
			await mocked.MockedControl.SetBrushInitialValueAsync (editor, new CommonSolidBrush (20, 120, 220, 240, "sRGB"));
			await mocked.MockedControl.SetMaterialDesignBrushInitialValueAsync (editor, new CommonSolidBrush (0x65, 0x1F, 0xFF, 200));
			await mocked.MockedControl.SetReadOnlyBrushInitialValueAsync (editor, new CommonSolidBrush (240, 220, 15, 190));
		}

		// If theme toggled, then notify our manager
		partial void OnThemeChanged (NSObject sender)
		{
			var themeControl = (NSSegmentedControl)sender;

			NSString appearance = NSAppearance.NameAqua;
			if (themeControl.SelectedSegment == 0) {
				if (NSProcessInfo.ProcessInfo.OperatingSystemVersion.Minor > 13)
					appearance = NSAppearance.NameDarkAqua;
				else
					appearance = NSAppearance.NameVibrantDark;
			}

			var realAppearance = NSAppearance.GetAppearance (appearance);
			((HostResourceProvider)PropertyPanel.HostResourceProvider).CurrentAppearance = realAppearance;
			View.Appearance = realAppearance;
		}
	}
}
