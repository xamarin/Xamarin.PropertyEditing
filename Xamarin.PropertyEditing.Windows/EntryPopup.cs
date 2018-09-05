using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Xamarin.PropertyEditing.Windows
{
	internal class EntryPopup
		: Popup
    {
		static EntryPopup ()
		{
			// Autocomplete is a popup in a popup and when you go to click the other popup, this one might close
			// so we need to hack around this otherwise uncontrollable behavior (StaysOpen has no effect).
			var existing = IsOpenProperty.GetMetadata (typeof(Popup));
			PropertyChangedCallback callback = (o,e) => {
				if ((bool) e.NewValue || ((EntryPopup) o).CanClose ()) {
					existing.PropertyChangedCallback (o, e);
				}
			};

			IsOpenProperty.OverrideMetadata(typeof(EntryPopup), new FrameworkPropertyMetadata (existing.DefaultValue, callback, existing.CoerceValueCallback));
		}

		public static readonly DependencyProperty ContentTemplateProperty = DependencyProperty.Register (
			nameof(ContentTemplate), typeof(DataTemplate), typeof(EntryPopup), new PropertyMetadata ((s,e) => ((EntryPopup)s).UpdateContentTemplate()));

		public DataTemplate ContentTemplate
		{
			get { return (DataTemplate)GetValue (ContentTemplateProperty); }
			set { SetValue (ContentTemplateProperty, value); }
		}

		public static readonly DependencyProperty ValueProperty = DependencyProperty.Register (
		    "Value", typeof(string), typeof(EntryPopup), new PropertyMetadata (default(string)));

		public string Value
		{
		    get { return (string) GetValue (ValueProperty); }
		    set { SetValue (ValueProperty, value); }
		}

		public override void OnApplyTemplate ()
		{
			base.OnApplyTemplate ();
			UpdateContentTemplate();
		}

		protected override void OnOpened (EventArgs e)
		{
			base.OnOpened (e);
			this.textBox.Focus();
		}

		protected override void OnClosed (EventArgs e)
		{
			if (!this.closingFromEscape) {
				GetBindingExpression (ValueProperty)?.UpdateSource ();
			} else
				this.closingFromEscape = false;

			base.OnClosed (e);
		}

		protected override void OnKeyDown (KeyEventArgs e)
		{
			base.OnKeyDown (e);

			if (e.Handled)
				return;

			if (e.Key == Key.Escape) {
				this.closingFromEscape = true;
				IsOpen = false;
				e.Handled = true;
			} else if (e.Key == Key.Enter) {
				IsOpen = false;
				e.Handled = true;
			}
		}

		private TextBoxEx textBox;
		private bool closingFromEscape;

		private bool CanClose ()
		{
			return this.textBox.CanCloseParent ();
		}

		private void UpdateContentTemplate()
		{
			Child = ContentTemplate?.LoadContent() as UIElement;
			if (Child == null)
				return;

			this.textBox = ((FrameworkElement)Child)?.FindName ("entry") as TextBoxEx;
			if (this.textBox == null)
				throw new InvalidOperationException ("Need an entry TextBoxEx for EntryPopup");
		}
	}
}