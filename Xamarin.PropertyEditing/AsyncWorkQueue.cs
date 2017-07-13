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
	/// 
	/// The expectation here is the queue will only ever end up being less than 10 items deep, so ~no attention is paid
	/// to performance for larger queues. If the intended use-cases end up with longer queues, we have a bigger problem.
	internal class AsyncWorkQueue
	{
		public Task<IDisposable> RequestAsyncWork (object requester)
		{
			AsyncValueWorker worker = new AsyncValueWorker (requester);
			this.workers.AddLast (worker.Node);
			if (this.workers.Count == 1)
				worker.Completion.SetResult (worker);
			else if (this.workers.Count > 1) {
				// If we're a child of the active worker, let us pass.
				var node = this.workers.First;
				while (node != null) {
					if (node.Value.Completion.Task.IsCompleted) {
						if (ReferenceEquals (node.Value.Requester, requester))
							worker.Completion.SetResult (worker);

						break;
					}

					node = node.Next;
				}
			}

			return worker.Completion.Task;
		}
		
		private readonly LinkedList<AsyncValueWorker> workers = new LinkedList<AsyncValueWorker>();

		private class AsyncValueWorker
			: IDisposable
		{
			public AsyncValueWorker (object requester)
			{
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
				LinkedListNode<AsyncValueWorker> toFree = null, right = Node.Next, left = Node.Previous;
				bool childActive = false;
				while (right != null || left != null) {
					if (ReferenceEquals (left?.Value.Requester, Requester) || ReferenceEquals (right?.Value.Requester, Requester)) {
						childActive = true;
						break;
					}

					if (toFree == null)
						toFree = left ?? right;

					left = left?.Previous;
					right = right?.Next;
				}

				if (!childActive)
					toFree?.Value.Completion.TrySetResult (toFree.Value);

				Node.List.Remove (Node);
			}
		}
	}
}