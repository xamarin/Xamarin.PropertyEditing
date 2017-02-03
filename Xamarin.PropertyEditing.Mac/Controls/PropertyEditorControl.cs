using System;
using AppKit;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	public abstract class PropertyEditorControl : NSView
	{
		public PropertyEditorControl ()
		{
		}

		public string Label { get; set; }

		PropertyViewModel viewModel;
		internal PropertyViewModel ViewModel {
			get { return viewModel; }
			set {
				if (viewModel == value)
					return;

				if (viewModel != null)
					viewModel.PropertyChanged -= HandlePropertyChanged;

				viewModel = value;
				UpdateModelValue ();
				viewModel.PropertyChanged += HandlePropertyChanged;
			}
		}

		protected abstract void UpdateModelValue ();

		protected abstract void HandlePropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e);
	}
}
