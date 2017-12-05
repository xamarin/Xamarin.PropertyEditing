using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Xamarin.PropertyEditing.Windows
{
	[TemplatePart (Name = "TextBox", Type = typeof(TextBox))]
	public class StringEditorControl
		: PropertyEditorControl
	{
		public override void OnApplyTemplate ()
		{
			base.OnApplyTemplate ();

			this.textBox = GetTemplateChild("TextBox") as TextBox;
			if (this.textBox == null)
				throw new InvalidOperationException ("StringEditorControl must have a 'TextBox' TextBox part");

			this.textBox.PreviewMouseLeftButtonDown += OnLeftDown;
			this.textBox.GotKeyboardFocus += OnGotFocus;
			this.textBox.MouseDoubleClick += OnGotFocus;
		}

		private TextBox textBox;

		private void OnGotFocus (object sender, EventArgs e)
		{
			if (this.textBox.SelectionLength == 0)
				this.textBox.SelectAll ();
		}

		private void OnLeftDown (object sender, MouseButtonEventArgs e)
		{
			if (this.textBox.IsKeyboardFocusWithin)
				return;

			e.Handled = true;
			this.textBox.Focus ();
			this.textBox.SelectAll ();
		}
	}
}
