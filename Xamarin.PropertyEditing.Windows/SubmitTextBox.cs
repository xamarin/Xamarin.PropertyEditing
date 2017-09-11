using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Xamarin.PropertyEditing.Windows
{
	/// <remarks>
	/// This textbox will update its Text binding also when enter is pressed
	/// </remarks>
    internal class SubmitTextBox
		: TextBox
    {
		public SubmitTextBox()
		{
			PreviewKeyDown += OnKeyDown;
		}

		public static readonly DependencyProperty FocusSelectsAllProperty = DependencyProperty.Register ("FocusSelectsAll", typeof (bool), typeof (SubmitTextBox));

		public bool FocusSelectsAll
		{
			get { return (bool)GetValue (FocusSelectsAllProperty); }
			set { SetValue (FocusSelectsAllProperty, value); }
		}

		protected override void OnPreviewMouseDown (MouseButtonEventArgs e)
		{
			if (!IsKeyboardFocusWithin) {
				e.Handled = true;
				Focus ();
			}

			base.OnPreviewMouseDown (e);
		}

		protected override void OnGotKeyboardFocus (KeyboardFocusChangedEventArgs e)
		{
			base.OnGotKeyboardFocus (e);

			if (FocusSelectsAll)
				SelectAll ();
		}

		private void OnKeyDown (object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key != Key.Enter)
				return;

			var expression = GetBindingExpression (TextProperty);
			expression?.UpdateSource ();
		}
	}
}
