using System;
using System.Collections.Generic;
using AppKit;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class BaseOutlineViewDataSource
		: NSOutlineViewDataSource
	{
		internal bool Filtering => !string.IsNullOrEmpty (DataContext.FilterText);
		internal PanelViewModel DataContext { get; }

		internal Dictionary<object, NSObjectFacade> GroupFacades { get; }

		internal BaseOutlineViewDataSource (PanelViewModel panelViewModel)
		{
			if (panelViewModel == null)
				throw new ArgumentNullException (nameof (panelViewModel));

			DataContext = panelViewModel;
			GroupFacades = new Dictionary<object, NSObjectFacade> ();
		}

		internal bool TryGetFacade (object element, out NSObjectFacade facade)
		{
			return GroupFacades.TryGetValue (element, out facade);
		}

		internal NSObjectFacade GetFacade (object element)
		{
			NSObjectFacade facade;
			if (element is PanelGroupViewModel) {
				if (!GroupFacades.TryGetValue (element, out facade)) {
					GroupFacades[element] = facade = new NSObjectFacade (element);
				}
			} else
				facade = new NSObjectFacade (element);

			return facade;
		}
	}
}
