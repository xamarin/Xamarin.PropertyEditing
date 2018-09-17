using System.Windows;
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
			var expression = GetBindingExpression (TextProperty);
			expression?.UpdateSource ();
		}

		private void Submit ()
		{
			if (!EnableSubmit)
				return;

			OnSubmit();
		}
	}
}
