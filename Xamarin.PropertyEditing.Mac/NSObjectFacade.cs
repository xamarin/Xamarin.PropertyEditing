using System;
using Foundation;

namespace Xamarin.PropertyEditing.Mac
{
	internal class NSObjectFacade
		: NSObject
	{
		public NSObjectFacade (object obj)
		{
			Target = obj;
		}

		public object Target
		{
			get;
		}
	}
}
