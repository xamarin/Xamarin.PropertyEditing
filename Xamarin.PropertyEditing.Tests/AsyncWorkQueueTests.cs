using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Xamarin.PropertyEditing.Tests
{
	[TestFixture]
	internal class AsyncWorkQueueTests
	{
		[SetUp]
		public void Setup ()
		{
			this.queue = new AsyncWorkQueue();
		}

		[Test, Timeout (1000)]
		public async Task FirstRequestIsCompleted ()
		{
			object requester = new object();
			await this.queue.RequestAsyncWork (requester);
		}

		[Test, Timeout (1000)]
		public async Task WorkContinuesOnDispose ()
		{
			object requester1 = new object();
			object requester2 = new	object();

			IDisposable work = await this.queue.RequestAsyncWork (requester1);
			Task<IDisposable> work2 = this.queue.RequestAsyncWork (requester2);

			work.Dispose();
			await work2;
			work2.Dispose();
		}

		[Test, Timeout (1000)]
		public async Task SameRequesterDoesntWait ()
		{
			object requester = new object();
			IDisposable work = await this.queue.RequestAsyncWork (requester);
			// If this was not safe to call from the same requester, the test should stop here and timeout
			IDisposable work2 = await this.queue.RequestAsyncWork (requester);

			work.Dispose();
			work2.Dispose();
		}

		[Test, Timeout (1000)]
		public async Task WorkContinuesOnDisposeOfParentAndChild ()
		{
			string requester1 = "r1";

			IDisposable parent = await this.queue.RequestAsyncWork (requester1);
			IDisposable child = await this.queue.RequestAsyncWork (requester1);

			string requester2 = "r2";
			Task work2 = this.queue.RequestAsyncWork (requester2);
			Assume.That (work2.IsCompleted, Is.False);

			child.Dispose();
			Assert.That (work2.IsCompleted, Is.False);

			parent.Dispose();
			await work2;
		}

		[Test, Timeout (1000)]
		public async Task WorkContinuesOnDisposeOfParentAndChildInterspersed ()
		{
			object requester1 = new object();
			IDisposable parent = await this.queue.RequestAsyncWork (requester1);

			object requester2 = new object();
			Task work2 = this.queue.RequestAsyncWork (requester2);
			Assume.That (work2.IsCompleted, Is.False);

			IDisposable child = await this.queue.RequestAsyncWork (requester1);

			child.Dispose();
			Assert.That (work2.IsCompleted, Is.False);

			parent.Dispose();
			await work2;
		}

		[Test, Timeout (1000)]
		public async Task WorkContinuesOnDisposeOfParentAndChildInterspersed2 ()
		{
			object requester1 = new object();
			IDisposable parent = await this.queue.RequestAsyncWork (requester1);

			object requester2 = new object();
			Task work2 = this.queue.RequestAsyncWork (requester2);
			Assume.That (work2.IsCompleted, Is.False);

			IDisposable child = await this.queue.RequestAsyncWork (requester1);

			parent.Dispose();
			Assert.That (work2.IsCompleted, Is.False);

			child.Dispose();
			await work2;
		}

		private AsyncWorkQueue queue;
	}
}