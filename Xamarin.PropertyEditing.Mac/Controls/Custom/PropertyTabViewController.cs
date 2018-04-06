using System;
using System.ComponentModel;
using AppKit;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	class PropertyTabViewController<T> : NSTabViewController where T : PropertyViewModel
	{
		public PropertyTabViewController () : base ()
		{
		}

		protected virtual void OnPropertyChanged (object sender, PropertyChangedEventArgs args)
		{
		}

		protected virtual void OnViewModelChanged (T oldModel)
		{
		}

		T viewModel;
		internal T ViewModel
		{
			get => viewModel;
			set
			{
				var oldModel = viewModel;
				if (viewModel == value)
					return;

				if (viewModel != null)
					viewModel.PropertyChanged -= OnPropertyChanged;

				viewModel = value;
				OnViewModelChanged (oldModel);

				viewModel.PropertyChanged += OnPropertyChanged;
			}
		}
	}
}
