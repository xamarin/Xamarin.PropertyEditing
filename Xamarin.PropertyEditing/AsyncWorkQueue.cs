using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Xamarin.PropertyEditing
{
	/// <remarks>
	/// This is NOT thread safe. It serves purely to keep async-waiting UI events in order without blocking the UI thread.
	/// The added capability here past just waiting on a task and swapping it with the new wait is child requests will pass
	/// through.
	/// </remarks>
	internal class AsyncWorkQueue
	{
		public Task<IDisposable> RequestAsyncWork (object requester)
		{
			var worker = new AsyncValueWorker (requester, this);
			this.workers.AddLast (worker.Node);

			if (this.workers.Count == 1 || (this.activeRequester.TryGetTarget (out var target) && ReferenceEquals (requester, target))) {
				worker.Completion.SetResult (worker);
				this.activeRequester.SetTarget (requester);
			}

			return worker.Completion.Task;
		}

		private WeakReference<object> activeRequester;
		private readonly LinkedList<AsyncValueWorker> workers = new LinkedList<AsyncValueWorker>();

		private void CompleteWork (AsyncValueWorker worker)
		{
			LinkedListNode<AsyncValueWorker> left = worker.Node.Previous, right = worker.Node.Next;
			AsyncValueWorker toFree = left?.Value ?? right?.Value;

			List<AsyncValueWorker> related = null;

			// If a worker is still in the queue, it's still working. All children/siblings of the
			// current requester must be finished before it can move on.
			while (left != null || right != null) {
				if (ReferenceEquals (left?.Value.Requester, worker.Requester) || ReferenceEquals (right?.Value.Requester, worker.Requester)) {
					toFree = null;
					break;
				}

				if (ReferenceEquals (left?.Value.Requester, toFree?.Requester)) {
					if (related == null)
						related = new List<AsyncValueWorker> ();

					related.Add (left.Value);
				}

				if (ReferenceEquals (right?.Value.Requester, toFree?.Requester)) {
					if (related == null)
						related = new List<AsyncValueWorker> ();

					related.Add (right.Value);
				}

				left = left?.Previous;
				right = right?.Next;
			}

			this.workers.Remove (worker);

			if (toFree != null) {
				this.activeRequester.SetTarget (toFree.Requester);
				toFree.Completion.SetResult (toFree);

				if (related != null) {
					// Once the active requester changes, we need to sure its children/siblings are all allowed
					// same as if it had been the active one first.
					foreach (var w in related) {
						w.Completion.TrySetResult (w);
					}
				}
			}
		}

		private class AsyncValueWorker
			: IDisposable
		{
			public AsyncValueWorker (object requester, AsyncWorkQueue queue)
			{
				this.queue = queue;
				Requester = requester;
				Node = new LinkedListNode<AsyncValueWorker> (this);
			}

			public object Requester
			{
				get;
			}

			public LinkedListNode<AsyncValueWorker> Node
			{
				get;
			}

			public TaskCompletionSource<IDisposable> Completion
			{
				get;
			} = new TaskCompletionSource<IDisposable> ();

			public void Dispose ()
			{
				if (this.isDisposed)
					return;

				this.isDisposed = true;
				this.queue.CompleteWork (this);
			}

			private bool isDisposed;
			private readonly AsyncWorkQueue queue;
		}
	}
}