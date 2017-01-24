using AppKit;

namespace Xamarin.PropertyEditing.Mac.Standalone
{
	static class MainClass
	{
		static void Main (string [] args)
		{
			var foo = typeof (PropertyEditorPanel);
			NSApplication.Init ();
			NSApplication.Main (args);
		}
	}
}
