using System;
namespace Xamarin.PropertyEditing.ViewModels
{
	internal class CreateResourceRequestedEventArgs
		: EventArgs
	{
		public ResourceSource Source
		{
			get;
			set;
		}

		public string Name
		{
			get;
			set;
		}
	}
}
