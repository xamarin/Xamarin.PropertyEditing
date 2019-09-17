using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls.Primitives;

namespace Xamarin.PropertyEditing.Windows
{
    internal class ToggleButtonEx
		: ToggleButton
    {
	    public static readonly DependencyProperty IsMouseOverRemoteProperty = DependencyProperty.Register (
		    "IsMouseOverRemote", typeof(bool), typeof(ToggleButtonEx), new PropertyMetadata (default(bool)));

	    public bool IsMouseOverRemote
	    {
		    get { return (bool) GetValue (IsMouseOverRemoteProperty); }
		    set { SetValue (IsMouseOverRemoteProperty, value); }
	    }

	    protected override AutomationPeer OnCreateAutomationPeer ()
	    {
		    return new ToggleButtonExAutomationPeer (this);
	    }

	    private class ToggleButtonExAutomationPeer
		    : ToggleButtonAutomationPeer
	    {
		    public ToggleButtonExAutomationPeer (ToggleButton owner)
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
