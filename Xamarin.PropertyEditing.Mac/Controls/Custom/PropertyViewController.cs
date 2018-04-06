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
	class PropertyViewController<T> : NSViewController where T : PropertyViewModel
	{
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

		protected virtual void OnViewModelChanged (T oldModel)
		{
		}

		protected virtual void OnPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
		}
	}
}
