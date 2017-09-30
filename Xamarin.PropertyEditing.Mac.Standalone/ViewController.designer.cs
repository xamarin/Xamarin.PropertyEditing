// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace Xamarin.PropertyEditing.Mac.Standalone
{
	[Register ("ViewController")]
	partial class ViewController
	{
		[Outlet]
		Xamarin.PropertyEditing.Mac.PropertyEditorPanel PropertyPanel { get; set; }

		[Action ("OnClickEvent:")]
		partial void OnClickEvent (Foundation.NSObject sender);

		[Action ("OnThemeChanged:")]
		partial void OnThemeChanged (Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (PropertyPanel != null) {
				PropertyPanel.Dispose ();
				PropertyPanel = null;
			}
		}
	}
}
