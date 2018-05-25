using AppKit;
using Foundation;

namespace Xamarin.PropertyEditing.Mac.Standalone
{
	[Register ("AppDelegate")]
	public class AppDelegate : NSApplicationDelegate
	{
		public AppDelegate ()
		{
		}

		public override void DidFinishLaunching (NSNotification notification)
		{
			BeginLooping();
			// Insert code here to initialize your application
		}

		async void BeginLooping ()
		{
			while (true) {

				var start = System.DateTime.Now;
				for (int i = 0; i < 500; i ++)
					(BooleanCreateor.Create () as System.IDisposable)?.Dispose ();

				var total = (System.DateTime.Now - start).TotalMilliseconds;
				System.Console.WriteLine ($"{total}ms total. {total/500} per control");
				await System.Threading.Tasks.Task.Delay (500);
			}
		}

		public override void WillTerminate (NSNotification notification)
		{
			// Insert code here to tear down your application
		}
	}
}
