using System;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class ResourceRequestedEventArgs
		: EventArgs
	{
		public Resource Resource
		{
			get;
			set;
		}
	}
}