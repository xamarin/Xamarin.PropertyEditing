using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;

namespace Xamarin.PropertyEditing.Windows
{
	internal class ButtonEx
		: Button
	{
		protected override AutomationPeer OnCreateAutomationPeer ()
		{
			return new ButtonExAutomationPeer (this);
		}

		private class ButtonExAutomationPeer
			: ButtonAutomationPeer
		{
			public ButtonExAutomationPeer (Button owner)
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
