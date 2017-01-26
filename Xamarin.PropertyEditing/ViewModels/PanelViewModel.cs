using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class PanelViewModel
		: PropertiesViewModel
	{
		public PanelViewModel (IEditorProvider provider)
			: base (provider)
		{
		}
	}
}