using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
    }
}
