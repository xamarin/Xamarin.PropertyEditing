using System;
using System.ComponentModel;
using AppKit;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	class NotifyingTabViewController<T> : NSTabViewController, INotifyingListner<T> where T : NotifyingObject
	{
		public NotifyingTabViewController () : base ()
		{
			Adaptor = new NotifyingViewAdaptor<T> (this);
		}

		protected NotifyingViewAdaptor<T> Adaptor { get; }

		public virtual void OnPropertyChanged (object sender, PropertyChangedEventArgs args)
		{
		}

		public virtual void OnViewModelChanged (T oldModel)
		{
		}

		internal T ViewModel {
			get => Adaptor.ViewModel;
			set => Adaptor.ViewModel = value;
		}
	}
}
