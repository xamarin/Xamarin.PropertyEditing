using System;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class CreateBindingRequestedEventArgs
		: EventArgs
	{
		public object BindingObject
		{
			get;
			set;
		}
	}
}