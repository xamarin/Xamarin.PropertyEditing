using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading;

namespace Xamarin.PropertyEditing.Tests
{
	internal class TestContext
		: SynchronizationContext
	{
		public override void Post (SendOrPostCallback d, object state)
		{
			try {
				d (state);
			} catch (Exception ex) {
				var info = ExceptionDispatchInfo.Capture (ex);
				this.exceptions.Add (info);

				// If we throw exceptions here we will crash out the test runner process when
				// running tests from VSM.
				//
				// As part of our `Teardown` method we manually rethrow the exceptions so they
				// propagate to NUnit and cause the test to fail.

				//throw;
			}
		}

		public override void Send (SendOrPostCallback d, object state)
		{
			try {
				d (state);
			} catch (Exception ex) {
				var info = ExceptionDispatchInfo.Capture (ex);
				this.exceptions.Add (info);

				// If we throw exceptions here we will crash out the test runner process when
				// running tests from VSM.
				//
				// As part of our `Teardown` method we manually rethrow the exceptions so they
				// propagate to NUnit and cause the test to fail.

				//throw;
			}
		}

		public void ThrowPendingExceptions ()
		{
			if (this.exceptions.Count > 0)
				this.exceptions[0].Throw();
		}

		private readonly List<ExceptionDispatchInfo> exceptions = new List<ExceptionDispatchInfo> ();
	}
}
