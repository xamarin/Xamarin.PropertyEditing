using System;
using System.Windows.Input;
using AppKit;

namespace Xamarin.PropertyEditing.Mac.Controls.Custom
{
	internal class CommandButton : NSButton
	{
		private ICommand command;

		public ICommand Command
		{
			get { return this.command; }
			set
			{
				if (this.command != null)
					this.command.CanExecuteChanged -= CanExecuteChanged;

				this.command = value;

				if (this.command != null)
					this.command.CanExecuteChanged += CanExecuteChanged;
			}
		}

		public CommandButton ()
		{
			Activated += (object sender, EventArgs e) => {
				var button = (CommandButton)sender;
				button.command?.Execute (null);
			};
		}

		private void CanExecuteChanged (object sender, EventArgs e)
		{
			if (sender is ICommand cmd)
				this.Enabled = cmd.CanExecute (null);
		}
	}
}
