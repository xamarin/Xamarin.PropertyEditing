using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Xamarin.PropertyEditing
{
	internal sealed class AsyncValue<T>
		: INotifyPropertyChanged
	{
		public AsyncValue (Task<T> task, T defaultValue = default(T))
		{
			if (task == null)
				throw new ArgumentNullException (nameof(task));

			this.task = task;
			this.defaultValue = defaultValue;

			TaskScheduler scheduler = TaskScheduler.Current;
			if (SynchronizationContext.Current != null)
				scheduler = TaskScheduler.FromCurrentSynchronizationContext ();

			this.task.ContinueWith (t => {
				IsRunning = false;
			}, scheduler);

			this.task.ContinueWith (t => {
				OnPropertyChanged (nameof(Value));
			}, CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, scheduler);
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public bool IsRunning
		{
			get { return this.isRunning; }
			set
			{
				if (this.isRunning == value)
					return;

				this.isRunning = value;
				OnPropertyChanged();
			}
		}

		public Task<T> Task => this.task;

		public T Value => (this.task.Status == TaskStatus.RanToCompletion) ? this.task.Result : this.defaultValue;

		private bool isRunning = true;
		private readonly Task<T> task;
		private readonly T defaultValue;

		private void OnPropertyChanged ([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke (this, new PropertyChangedEventArgs (propertyName));
		}
	}
}