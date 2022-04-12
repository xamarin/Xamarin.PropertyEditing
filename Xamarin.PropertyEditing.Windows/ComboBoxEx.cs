using System.Windows;
using System.Windows.Automation;
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

		protected override AutomationPeer OnCreateAutomationPeer ()
		{
			return new ComboBoxExAutomationPeer (this);
		}

		public override void OnApplyTemplate ()
		{
			base.OnApplyTemplate ();

			var textBox = Template.FindName ("PART_EditableTextBox", this) as TextBox;
			if (textBox != null) {
				string accessibilityName = AutomationProperties.GetName (this);

				AutomationProperties.SetName (textBox, accessibilityName);
			}
		}

		protected override void OnSelectionChanged (SelectionChangedEventArgs e)
		{
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
			var expression = GetBindingExpression (TextProperty);
			expression?.UpdateSource ();
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
