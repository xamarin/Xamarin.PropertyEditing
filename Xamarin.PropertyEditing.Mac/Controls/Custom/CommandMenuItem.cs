using System;
using System.Windows.Input;
using AppKit;

namespace Xamarin.PropertyEditing.Mac
{
	internal class CommandMenuItem
		: ThemedMenuItem
	{
		public CommandMenuItem (IHostResourceProvider hostResources, string title)
			: base (hostResources, title)
		{
			HookUpCommandEvents ();
		}

		public CommandMenuItem (IHostResourceProvider hostResources, string title, ICommand command)
			: this (hostResources, title)
		{
			if (command == null)
				throw new ArgumentNullException (nameof (command));

			Command = command;
		}

		ICommand command;

		public ICommand Command {
			get { return command; }
			set {
				if (this.command != null)
					this.command.CanExecuteChanged -= CanExecuteChanged;

				this.command = value;

				if (this.command != null)
					this.command.CanExecuteChanged += CanExecuteChanged;
			}
		}

		private void HookUpCommandEvents ()
		{
			Activated += (object sender, EventArgs e) => {
				if (this.command != null) {
						this.command.Execute (null);
				}
			};

			ValidateMenuItem = ValidatePropertyMenuItem;
		}

		private bool ValidatePropertyMenuItem (NSMenuItem menuItem)
		{
			if (this.command != null)
				return this.command.CanExecute (null);

			return false;
		}

		private void CanExecuteChanged (object sender, EventArgs e)
		{
			if (sender is ICommand cmd)
				this.Enabled = cmd.CanExecute (null);
		}
	}
}
