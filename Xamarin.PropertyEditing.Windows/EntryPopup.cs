using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Xamarin.PropertyEditing.Windows
{
	internal class EntryPopup
		: Popup
    {
		public static readonly DependencyProperty ContentTemplateProperty = DependencyProperty.Register (
			nameof(ContentTemplate), typeof(DataTemplate), typeof(EntryPopup), new PropertyMetadata ((s,e) => ((EntryPopup)s).UpdateContentTemplate()));

		public DataTemplate ContentTemplate
		{
			get { return (DataTemplate)GetValue (ContentTemplateProperty); }
			set { SetValue (ContentTemplateProperty, value); }
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
				this.textBox.GetBindingExpression (TextBox.TextProperty)?.UpdateSource();
			} else
				this.closingFromEscape = false;

			base.OnClosed (e);
		}

		protected override void OnPreviewKeyDown (KeyEventArgs e)
		{
			if (e.Key == Key.Escape) {
				this.closingFromEscape = true;
				IsOpen = false;
				e.Handled = true;
			} else if (e.Key == Key.Enter) {
				IsOpen = false;
				e.Handled = true;
			}

			base.OnPreviewKeyDown (e);
		}

		private TextBox textBox;
		private bool closingFromEscape;

		private void UpdateContentTemplate()
		{
			Child = ContentTemplate?.LoadContent() as UIElement;
			if (Child == null)
				return;

			this.textBox = ((FrameworkElement)Child)?.FindName ("entry") as TextBox;
			if (this.textBox == null)
				throw new InvalidOperationException ("Need an entry TextBox for EntryPopup");
		}
	}
}