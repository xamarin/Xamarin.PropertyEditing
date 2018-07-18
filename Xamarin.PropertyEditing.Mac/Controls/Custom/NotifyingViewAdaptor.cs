using System;
using System.ComponentModel;

namespace Xamarin.PropertyEditing.Mac
{
	internal interface INotifyingListner<T> where T : NotifyingObject
	{
		void OnViewModelChanged (T oldModel);
		void OnPropertyChanged (object sender, PropertyChangedEventArgs e);
	}

	internal class NotifyingViewAdaptor<T> 
		: IDisposable
		where T : NotifyingObject
	{
		public NotifyingViewAdaptor (INotifyingListner<T> listener)
		{
			this.listener = listener;
		}

		private INotifyingListner<T> listener;

		private T viewModel;
		internal T ViewModel {
			get => this.viewModel;
			set {
				var oldModel = this.viewModel;
				if (this.viewModel == value)
					return;

				if (oldModel != null)
					oldModel.PropertyChanged -= OnPropertyChanged;

				this.viewModel = value;

				OnViewModelChanged (oldModel);
				if (this.viewModel == null)
					return;

				this.viewModel.PropertyChanged += OnPropertyChanged;
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

		public void Dispose ()
		{
			if (this.viewModel != null)
				this.viewModel.PropertyChanged -= OnPropertyChanged;
		}
	}
}
