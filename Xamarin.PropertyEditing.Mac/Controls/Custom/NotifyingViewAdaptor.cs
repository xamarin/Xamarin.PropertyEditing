using System;
using System.ComponentModel;

namespace Xamarin.PropertyEditing.Mac
{
	internal interface INotifyingListner<T> where T : NotifyingObject
	{
		void OnViewModelChanged (T oldModel);
		void OnPropertyChanged (object sender, PropertyChangedEventArgs e);
	}

	internal class NotifyingViewAdaptor<T> where T : NotifyingObject
	{
		public NotifyingViewAdaptor (INotifyingListner<T> listener)
		{
			this.listener = listener;
		}

		private INotifyingListner<T> listener;

		private T viewModel;
		internal T ViewModel {
			get => viewModel;
			set {
				var oldModel = viewModel;
				if (viewModel == value)
					return;

				if (oldModel != null)
					oldModel.PropertyChanged -= OnPropertyChanged;

				viewModel = value;

				OnViewModelChanged (oldModel);
				if (viewModel == null)
					return;
				
				viewModel.PropertyChanged += OnPropertyChanged;
			}
		}

		public void OnViewModelChanged (T oldModel)
		{
			this.listener.OnViewModelChanged (oldModel);
		}

		public void OnPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			this.listener.OnPropertyChanged (sender, e);
		}

		public void Disconnect ()
		{
			if (this.viewModel != null)
				this.viewModel.PropertyChanged -= OnPropertyChanged;
		}
	}
}
