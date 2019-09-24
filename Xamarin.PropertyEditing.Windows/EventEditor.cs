using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;

namespace Xamarin.PropertyEditing.Windows
{
    internal class EventEditor
		: Control
    {
	    public static readonly DependencyProperty AddCommandProperty = DependencyProperty.Register (
		    "AddCommand", typeof(ICommand), typeof(EventEditor), new PropertyMetadata (default(ICommand)));

	    public ICommand AddCommand
	    {
		    get { return (ICommand) GetValue (AddCommandProperty); }
		    set { SetValue (AddCommandProperty, value); }
	    }

	    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register (
		    "ItemsSource", typeof(object), typeof(EventEditor), new PropertyMetadata (default(object)));

	    public object ItemsSource
	    {
		    get { return (object) GetValue (ItemsSourceProperty); }
		    set { SetValue (ItemsSourceProperty, value); }
	    }

	    public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register (
		    "ItemTemplate", typeof(DataTemplate), typeof(EventEditor), new PropertyMetadata (default(DataTemplate)));

	    public DataTemplate ItemTemplate
	    {
		    get { return (DataTemplate) GetValue (ItemTemplateProperty); }
		    set { SetValue (ItemTemplateProperty, value); }
	    }

	    protected override AutomationPeer OnCreateAutomationPeer ()
	    {
		    return new EditorAutomationPeer (this);
	    }
    }
}
