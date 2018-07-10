using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using AppKit;
using CoreAnimation;
using CoreGraphics;
using Xamarin.PropertyEditing.Drawing;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	class NotifyingViewController<T> : NSViewController, INotifyingListner<T> where T : NotifyingObject
	{
		internal T ViewModel {
			get => Adaptor.ViewModel;
			set => Adaptor.ViewModel = value;
		}

		public NotifyingViewController ()
		{
			Adaptor = new NotifyingViewAdaptor<T> (this);
		}

		protected NotifyingViewAdaptor<T> Adaptor { get; }

		public virtual void OnViewModelChanged (T oldModel)
		{
		}

		public virtual void OnPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
		}
	}
}
