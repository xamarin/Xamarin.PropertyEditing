using System;
using System.Windows.Input;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class RelayCommand<T>
		: ICommand
	{
		public RelayCommand (Action<T> execute)
		{
			if (execute == null)
				throw new ArgumentNullException (nameof (execute));

			this.execute = execute;
		}

		public RelayCommand (Action<T> execute, Func<T, bool> canExecute)
			: this (execute)
		{
			if (canExecute == null)
				throw new ArgumentNullException (nameof (canExecute));

			this.canExecute = canExecute;
		}

		public event EventHandler CanExecuteChanged;

		public bool CanExecute (object parameter)
		{
			if (this.canExecute == null)
				return true;

			return this.canExecute ((T) parameter);
		}

		public void Execute (object parameter)
		{
			this.execute ((T) parameter);
		}

		public void ChangeCanExecute ()
		{
			CanExecuteChanged?.Invoke (this, EventArgs.Empty);
		}

		private readonly Func<T, bool> canExecute;
		private readonly Action<T> execute;
	}

	internal class RelayCommand
		: ICommand
	{
		public RelayCommand (Action execute)
		{
			if (execute == null)
				throw new ArgumentNullException (nameof (execute));

			this.execute = execute;
		}

		public RelayCommand (Action execute, Func<bool> canExecute)
			: this (execute)
		{
			if (canExecute == null)
				throw new ArgumentNullException (nameof (canExecute));

			this.canExecute = canExecute;
		}

		public event EventHandler CanExecuteChanged;

		public bool CanExecute (object parameter)
		{
			if (this.canExecute == null)
				return true;

			return this.canExecute();
		}

		public void Execute (object parameter)
		{
			this.execute();
		}

		public void ChangeCanExecute ()
		{
			CanExecuteChanged?.Invoke (this, EventArgs.Empty);
		}

		private readonly Func<bool> canExecute;
		private readonly Action execute;
	}
}