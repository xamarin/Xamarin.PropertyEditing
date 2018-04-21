using System;
using System.Collections.Generic;
using AppKit;
using Foundation;
using Xamarin.PropertyEditing.Tests;

namespace Xamarin.PropertyEditing.Mac.Standalone
{
	public partial class ViewController : NSViewController
	{
		public ViewController (IntPtr handle) : base (handle)
		{
			
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			// Do any additional setup after loading the view.

			PropertyPanel.TargetPlatform = new TargetPlatform {
				SupportsCustomExpressions = true,
				GroupedTypes = new Dictionary<Type, string> {
					// TODO Add Types
				}
			};

			PropertyPanel.EditorProvider = new MockEditorProvider ();
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

		// load panel from active designer item, clear it if none selected
		partial void OnClickEvent (NSObject sender)
		{
			var clickedButton = sender as NSButton;
			var mockedButton = clickedButton?.Cell as IMockedControl;
			var inspectedObject = (mockedButton == null || mockedButton.MockedControl == null)
				? (object)sender : mockedButton.MockedControl;
			if (PropertyPanel.SelectedItems.Contains (inspectedObject)) {
				PropertyPanel.SelectedItems.Remove (inspectedObject);
			} else {
				PropertyPanel.SelectedItems.Add (inspectedObject);
			}
		}

		// If theme toggled, then notify our manager
		partial void OnThemeChanged (NSObject sender)
		{
			var themeControl = sender as NSSegmentedControl;
			switch (themeControl.SelectedSegment) {
				case 0:
					PropertyEditorPanel.ThemeManager.Theme = Themes.PropertyEditorTheme.Dark;
					break;
				case 1:
					PropertyEditorPanel.ThemeManager.Theme = Themes.PropertyEditorTheme.Light;
					break;
				case 2:
					PropertyEditorPanel.ThemeManager.Theme = Themes.PropertyEditorTheme.None;
					break;
			}
		}
	}
}
