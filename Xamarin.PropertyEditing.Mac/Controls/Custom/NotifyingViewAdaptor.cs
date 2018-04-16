using System;
using System.ComponentModel;

namespace Xamarin.PropertyEditing.Mac
{

	interface INotifyingListner<T> where T : NotifyingObject
	{
		void OnViewModelChanged (T oldModel);
		void OnPropertyChanged (object sender, PropertyChangedEventArgs e);
	}

	class NotifyingViewAdaptor<T> where T : NotifyingObject
	{
		public NotifyingViewAdaptor (INotifyingListner<T> listener)
		{
			this.listener = listener;
		}

		INotifyingListner<T> listener;

		T viewModel;
		internal T ViewModel
		{
			get => viewModel;
			set
			{
				var oldModel = viewModel;
				if (viewModel == value)
					return;

				if (oldModel != null)
					oldModel.PropertyChanged -= OnPropertyChanged;

				viewModel = value;

				OnViewModelChanged (oldModel);
				viewModel.PropertyChanged += OnPropertyChanged;
			}
		}

		public void OnViewModelChanged (T oldModel)
		{
			listener.OnViewModelChanged (oldModel);
		}

		public void OnPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			listener.OnPropertyChanged (sender, e);
		}

		public void Disconnect ()
		{
			if (viewModel != null)
				viewModel.PropertyChanged -= OnPropertyChanged;
		}
	}
}
