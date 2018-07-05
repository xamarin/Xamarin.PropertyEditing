using System.Threading;

namespace Xamarin.PropertyEditing.Tests
{
	internal class AsyncSynchronizationContext
		: SynchronizationContext
	{
		public override void Post (SendOrPostCallback d, object state)
		{
			d (state);
		}

		public void WaitForPendingOperationsToComplete ()
		{
		}
	}
}