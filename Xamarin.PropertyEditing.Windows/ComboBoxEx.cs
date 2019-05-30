using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;

namespace Xamarin.PropertyEditing.Windows
{
	internal class ComboBoxEx
		: ComboBox
	{
		static ComboBoxEx ()
		{
			DefaultStyleKeyProperty.OverrideMetadata (typeof (ComboBoxEx), new FrameworkPropertyMetadata (typeof (ComboBoxEx)));
		}

		public static readonly DependencyProperty EnableSubmitProperty = DependencyProperty.Register (
			"EnableSubmit", typeof (bool), typeof (ComboBoxEx), new PropertyMetadata (true));

		public bool EnableSubmit
		{
			get { return (bool)GetValue (EnableSubmitProperty); }
			set { SetValue (EnableSubmitProperty, value); }
		}

		public static readonly DependencyProperty SubmitCommandProperty = DependencyProperty.Register (
			"SubmitCommand", typeof(ICommand), typeof(ComboBoxEx), new PropertyMetadata (default(ICommand)));

		public ICommand SubmitCommand
		{
			get { return (ICommand) GetValue (SubmitCommandProperty); }
			set { SetValue (SubmitCommandProperty, value); }
		}

		public static readonly DependencyProperty ClearOnSubmitProperty = DependencyProperty.Register (
			"ClearOnSubmit", typeof(bool), typeof(ComboBoxEx), new PropertyMetadata (default(bool)));

		public bool ClearOnSubmit
		{
			get { return (bool) GetValue (ClearOnSubmitProperty); }
			set { SetValue (ClearOnSubmitProperty, value); }
		}

		protected override AutomationPeer OnCreateAutomationPeer ()
		{
			return new ComboBoxExAutomationPeer (this);
		}

		protected override void OnSelectionChanged (SelectionChangedEventArgs e)
		{
			if (!IsDropDownOpen)
				return;

			base.OnSelectionChanged (e);
			Submit();
		}

		protected override void OnKeyDown (KeyEventArgs e)
		{
			if (e.Key == Key.Enter) {
				Submit ();
				e.Handled = true;
			}

			base.OnKeyDown (e);
		}

		protected override void OnLostKeyboardFocus (KeyboardFocusChangedEventArgs e)
		{
			base.OnLostKeyboardFocus (e);
			Submit ();
		}

		protected virtual void OnSubmit ()
		{
			ICommand command = SubmitCommand;
			if (command != null) {
				if (command.CanExecute (Text))
					command.Execute (Text);
			} else {
				var expression = GetBindingExpression (TextProperty);
				expression?.UpdateSource ();
			}

			if (ClearOnSubmit)
				Text = null;
		}

		private void Submit ()
		{
			if (!EnableSubmit)
				return;

			OnSubmit();
		}

		private class ComboBoxExAutomationPeer
			: ComboBoxAutomationPeer
		{
			public ComboBoxExAutomationPeer (ComboBox owner)
				: base (owner)
			{
			}

			protected override bool IsControlElementCore ()
			{
				return base.IsControlElementCore () && Owner.IsVisible;
			}
		}
	}
}
