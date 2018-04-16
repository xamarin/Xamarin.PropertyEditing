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
	class PropertyViewController<T> : NSViewController, INotifyingListner<T> where T : NotifyingObject
	{
		internal T ViewModel
		{
			get => adaptor.ViewModel;
			set => adaptor.ViewModel = value;
		}

		public PropertyViewController ()
		{
			adaptor = new NotifyingViewAdaptor<T> (this);
		}

		protected NotifyingViewAdaptor<T> adaptor { get; }

		public virtual void OnViewModelChanged (T oldModel)
		{
		}

		public virtual void OnPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
		}
	}
}
