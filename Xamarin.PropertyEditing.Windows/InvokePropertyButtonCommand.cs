using System;
using System.Windows.Input;

namespace Xamarin.PropertyEditing.Windows
{
	internal class InvokePropertyButtonCommand
		: ICommand
	{
		public event EventHandler CanExecuteChanged;

		public bool CanExecute (object parameter)
		{
			return (parameter is PropertyButton);
		}

		public void Execute (object parameter)
		{
			((PropertyButton)parameter).ShowMenu();
		}
	}
}
