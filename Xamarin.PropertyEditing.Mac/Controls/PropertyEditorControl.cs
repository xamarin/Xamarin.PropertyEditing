using System;
using System.Collections;
using AppKit;
using CoreGraphics;
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

				if (viewModel != null) {
					viewModel.PropertyChanged -= HandlePropertyChanged;
					viewModel.ErrorsChanged -= HandleErrorsChanged;
				}

				viewModel = value;
				UpdateModelValue ();
				viewModel.PropertyChanged += HandlePropertyChanged;

				// FIXME: figure out what we want errors to display as (tooltip, etc.)
				viewModel.ErrorsChanged += HandleErrorsChanged;
			}
		}

		protected virtual void UpdateModelValue ()
		{
			SetEnabled ();
			UpdateAccessibilityValues ();
		}

		protected abstract void HandlePropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e);

		/// <summary>
		/// Update the display with any errors we need to show or remove
		/// </summary>
		/// <param name="errors">The error messages to display to the user</param>
		protected abstract void UpdateErrorsDisplayed (IEnumerable errors);

		protected abstract void HandleErrorsChanged (object sender, System.ComponentModel.DataErrorsChangedEventArgs e);

		protected abstract void SetEnabled ();

		protected abstract void UpdateAccessibilityValues ();
	}
}
