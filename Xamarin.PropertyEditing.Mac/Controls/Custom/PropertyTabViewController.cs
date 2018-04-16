using System;
using System.ComponentModel;
using AppKit;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	class PropertyTabViewController<T> : NSTabViewController, INotifyingListner<T> where T : NotifyingObject
	{
		public PropertyTabViewController () : base ()
		{
			adaptor = new NotifyingViewAdaptor<T> (this);
		}

		protected NotifyingViewAdaptor<T> adaptor { get; } 

		public virtual void OnPropertyChanged (object sender, PropertyChangedEventArgs args)
		{
		}

		public virtual void OnViewModelChanged (T oldModel)
		{
		}

		internal T ViewModel
		{
			get => adaptor.ViewModel;
			set => adaptor.ViewModel = value;
		}
	}
}
