using System;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Xamarin.PropertyEditing.Windows
{
	[TemplatePart (Name ="PART_Clear", Type = typeof (Button))]
	[TemplatePart (Name = "PART_CompletePopup", Type = typeof (Popup))]
	[TemplatePart (Name = "PART_CompleteList", Type = typeof (ListBox))]
	internal class TextBoxEx
		: TextBox
	{

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

		public static readonly DependencyProperty EnableClearProperty = DependencyProperty.Register (
			"EnableClear", typeof(bool), typeof(TextBoxEx), new PropertyMetadata (true));

		public bool EnableClear
		{
			get { return (bool) GetValue (EnableClearProperty); }
			set { SetValue (EnableClearProperty, value); }
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

		public static readonly DependencyProperty EnableSubmitProperty = DependencyProperty.Register (
			"EnableSubmit", typeof(bool), typeof(TextBoxEx), new PropertyMetadata (true));

		public bool EnableSubmit
		{
			get { return (bool) GetValue (EnableSubmitProperty); }
			set { SetValue (EnableSubmitProperty, value); }
		}

		public static readonly DependencyProperty SubmitButtonStyleProperty = DependencyProperty.Register (
			"SubmitButtonStyle", typeof(Style), typeof(TextBoxEx), new PropertyMetadata (default(Style)));

		public Style SubmitButtonStyle
		{
			get { return (Style) GetValue (SubmitButtonStyleProperty); }
			set { SetValue (SubmitButtonStyleProperty, value); }
		}

		public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register (
			"ItemsSource", typeof (IEnumerable), typeof (TextBoxEx), new PropertyMetadata (default (IEnumerable)));

		public IEnumerable ItemsSource
		{
			get { return (IEnumerable)GetValue (ItemsSourceProperty); }
			set { SetValue (ItemsSourceProperty, value); }
		}

		public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register (
			"ItemTemplate", typeof (DataTemplate), typeof (TextBoxEx), new PropertyMetadata (default (DataTemplate)));

		public DataTemplate ItemTemplate
		{
			get { return (DataTemplate)GetValue (ItemTemplateProperty); }
			set { SetValue (ItemTemplateProperty, value); }
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

			this.popup = (Popup)GetTemplateChild ("PART_CompletePopup");
			this.list = (ListBox)GetTemplateChild ("PART_CompleteList");
			if (this.popup == null || this.list == null)
				throw new InvalidOperationException ("PART_CompletePopup and PART_CompleteList must be present");

			this.list.ItemContainerGenerator.ItemsChanged += OnItemsChanged;
			this.list.PreviewMouseLeftButtonDown += OnListMouseDown;

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
			if (this.defocusFromList)
				return;

			base.OnLostKeyboardFocus (e);
			this.popup.IsOpen = false;

			if (EnableSubmit)
				OnSubmit ();
		}

		protected virtual void OnSubmit()
		{
			var expression = GetBindingExpression (TextProperty);
			expression?.UpdateSource ();
		}

		protected override void OnPreviewKeyDown (KeyEventArgs e)
		{
			e.Handled = true;
			if (e.Key == Key.Down) {
				if (this.list.SelectedIndex == -1 || this.list.SelectedIndex + 1 == this.list.Items.Count)
					this.list.SelectedIndex = 0;
				else
					this.list.SelectedIndex++;

				this.list.ScrollIntoView (this.list.SelectedItem);
			} else if (e.Key == Key.Up) {
				if (this.list.SelectedIndex == -1 || this.list.SelectedIndex == 0)
					this.list.SelectedIndex = this.list.Items.Count - 1;
				else
					this.list.SelectedIndex--;

				this.list.ScrollIntoView (this.list.SelectedItem);
			} else if (e.Key == Key.Enter || e.Key == Key.Tab) {
				if (this.list.SelectedValue != null) {
					SelectCompleteItem (this.list.SelectedItem);
				} else if (!this.popup.IsOpen) {
					if (EnableSubmit)
						OnSubmit ();
					else
						e.Handled = false;
				}
			} else if (e.Key == Key.Escape) {
				if (this.popup.IsOpen)
					this.popup.IsOpen = false;
				else if (EnableClear)
					Clear ();
				else
					e.Handled = false;
			} else {
				e.Handled = false;
			}

			base.OnPreviewKeyDown (e);
		}

		protected internal bool CanCloseParent ()
		{
			bool can = !this.defocusFromList;
			this.defocusFromList = false;
			return can;
		}

		private bool defocusFromList;
		private Popup popup;
		private ListBox list;

		private void SelectCompleteItem (object item)
		{
			Text = item.ToString ();
			CaretIndex = Text.Length;
			this.popup.IsOpen = false;
		}

		private void FocusSelect()
		{
			if (!FocusSelectsAll)
				return;

			if (SelectionLength == 0)
				SelectAll ();
		}

		private void OnItemsChanged (object sender, ItemsChangedEventArgs e)
		{
			if (!HasEffectiveKeyboardFocus)
				return;

			this.popup.IsOpen = (this.list.Items.Count > 0);
			if (this.list.SelectedIndex == -1)
				this.list.SelectedIndex = 0;
		}

		private void OnListMouseDown (object sender, MouseButtonEventArgs e)
		{
			Point pos = e.GetPosition (this.list);
			var element = this.list.InputHitTest (pos) as FrameworkElement;
			var item = element?.FindParentOrSelf<ListBoxItem> ();
			if (item == null)
				return;

			SelectCompleteItem (item.DataContext);
			this.defocusFromList = true;
			e.Handled = true;
		}
	}
}
