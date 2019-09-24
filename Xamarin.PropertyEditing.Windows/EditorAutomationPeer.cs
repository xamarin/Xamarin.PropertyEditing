using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;

using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Windows
{
	internal class EditorAutomationPeer
		: FrameworkElementAutomationPeer
	{
		public EditorAutomationPeer (FrameworkElement owner)
			: base (owner)
		{
			Refresh();
		}

		public void Refresh ()
		{
			this.name = AutomationProperties.GetName (Element);
			if (String.IsNullOrEmpty (this.name))
				this.name = (Element.DataContext as EditorViewModel)?.Name;
		}

		protected FrameworkElement Element => (FrameworkElement) Owner;

		protected override AutomationControlType GetAutomationControlTypeCore ()
		{
			return AutomationControlType.Group;
		}

		protected override string GetNameCore ()
		{
			return this.name;
		}
		
		private string name;
	}
}
