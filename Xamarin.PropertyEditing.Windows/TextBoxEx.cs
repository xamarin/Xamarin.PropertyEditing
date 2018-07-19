using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Xamarin.PropertyEditing.Windows
{
	[TemplatePart (Name ="PART_Clear", Type = typeof (Button))]
	internal class TextBoxEx
		: TextBox
	{
		public TextBoxEx()
		{
			PreviewKeyDown += OnPreviewKeyDown;
		}

		public static readonly DependencyProperty HintProperty = DependencyProperty.Register (
			"Hint", typeof(object), typeof(TextBoxEx), new PropertyMetadata (default(object)));

		public object Hint
		{
			get { return (object) GetValue (HintProperty); }
			set { SetValue (HintProperty, value); }
		}

		public static readonly DependencyProperty HintTemplateProperty = DependencyProperty.Register (
			"HintTemplate", typeof(DataTemplate), typeof(TextBoxEx), new PropertyMetadata (default(DataTemplate)));

		public DataTemplate HintTemplate
		{
			get { return (DataTemplate) GetValue (HintTemplateProperty); }
			set { SetValue (HintTemplateProperty, value); }
		}

		public static readonly DependencyProperty FocusSelectsAllProperty = DependencyProperty.Register (
			"FocusSelectsAll", typeof(bool), typeof(TextBoxEx), new PropertyMetadata (default(bool)));

		public bool FocusSelectsAll
		{
			get { return (bool) GetValue (FocusSelectsAllProperty); }
			set { SetValue (FocusSelectsAllProperty, value); }
		}

		public static readonly DependencyProperty ShowClearButtonProperty = DependencyProperty.Register (
			"ShowClearButton", typeof(bool), typeof(TextBoxEx), new PropertyMetadata (default(bool)));

		public bool ShowClearButton
		{
			get { return (bool) GetValue (ShowClearButtonProperty); }
			set { SetValue (ShowClearButtonProperty, value); }
		}

		public static readonly DependencyProperty ClearButtonStyleProperty = DependencyProperty.Register (
			"ClearButtonStyle", typeof(Style), typeof(TextBoxEx), new PropertyMetadata (default(Style)));

		public Style ClearButtonStyle
		{
			get { return (Style) GetValue (ClearButtonStyleProperty); }
			set { SetValue (ClearButtonStyleProperty, value); }
		}

		public static readonly DependencyProperty SubmitButtonStyleProperty = DependencyProperty.Register (
			"SubmitButtonStyle", typeof(Style), typeof(TextBoxEx), new PropertyMetadata (default(Style)));

		public Style SubmitButtonStyle
		{
			get { return (Style) GetValue (SubmitButtonStyleProperty); }
			set { SetValue (SubmitButtonStyleProperty, value); }
		}

		public override void OnApplyTemplate ()
		{
			base.OnApplyTemplate ();
			
			var clear = GetTemplateChild ("PART_Clear") as Button;
			if (clear == null) {
				if (!DesignerProperties.GetIsInDesignMode (this))
					throw new InvalidOperationException ("PART_Clear must be present as a button");

				return;
			}

			clear.Click += (sender, e) => {
				Clear();
			};
		}

		protected override void OnPreviewMouseLeftButtonDown (MouseButtonEventArgs e)
		{
			if (!FocusSelectsAll || IsKeyboardFocusWithin)
				return;

			e.Handled = true;
			Focus ();
			SelectAll ();

			base.OnPreviewMouseLeftButtonDown (e);
		}

		protected override void OnGotKeyboardFocus (KeyboardFocusChangedEventArgs e)
		{
			base.OnGotKeyboardFocus (e);

			FocusSelect ();
		}

		protected override void OnMouseDoubleClick (MouseButtonEventArgs e)
		{
			base.OnMouseDoubleClick (e);

			FocusSelect ();
		}

		protected override void OnLostKeyboardFocus (KeyboardFocusChangedEventArgs e)
		{
			base.OnLostKeyboardFocus (e);
			OnSubmit();
		}

		protected virtual void OnSubmit()
		{
			var expression = GetBindingExpression (TextProperty);
			expression?.UpdateSource ();
		}

		private void FocusSelect()
		{
			if (!FocusSelectsAll)
				return;

			if (SelectionLength == 0)
				SelectAll ();
		}

		private void OnPreviewKeyDown (object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == Key.Enter) {
				OnSubmit();
			} else if (e.Key == Key.Escape) {
				Clear();
			}
		}
	}
}
