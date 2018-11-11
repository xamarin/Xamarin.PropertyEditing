using System;
using System.Collections.Generic;
using AppKit;
using Foundation;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac.Controls
{
	internal class AutocompleteComboBox<T> : NSComboBox
	{
		protected PropertyViewModel<T> viewModel;

		public AutocompleteComboBox (PropertyViewModel<T> viewModel)
		{
			if (viewModel == null)
				throw new ArgumentNullException (nameof (viewModel));

			// This triggers autocomplete on items
			Completes = true;

			// Maximum number of items in the drop-down before scrolling kicks in.
			VisibleItems = 8;

			this.viewModel = viewModel;

			// It should be null, but belts and braces
			if (this.viewModel.AutocompleteItems != null) {
				PopulateComboBoxItems ();
			}
		}

		private void PopulateComboBoxItems ()
		{
			RemoveAll ();

			foreach (var item in this.viewModel.AutocompleteItems) {
				Add (new NSString (item));
			}
		}
	}
}
