using System;
using System.Collections;

using AppKit;
using Foundation;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal abstract class PropertyEditorControl : NSView
	{
		public string Label { get; set; }

		public abstract NSView FirstKeyView { get; }
		public abstract NSView LastKeyView { get; }

		public nint TableRow { get; set; } = -1;
		public NSTableView TableView { get; set; }
		public nfloat RowHeight { get; set; } = 24;

		PropertyViewModel viewModel;
		public PropertyViewModel ViewModel {
			get { return viewModel; }
			set {
				if (viewModel == value)
					return;

				PropertyViewModel oldModel = this.viewModel;
				if (oldModel != null) {
					oldModel.PropertyChanged -= OnPropertyChanged;
					oldModel.ErrorsChanged -= HandleErrorsChanged;
				}

				this.viewModel = value;
				OnViewModelChanged (oldModel);
				viewModel.PropertyChanged += OnPropertyChanged;

				// FIXME: figure out what we want errors to display as (tooltip, etc.)
				viewModel.ErrorsChanged += HandleErrorsChanged;
			}
		}

		[Export ("_primitiveSetDefaultNextKeyView:")]
		public void SetDefaultNextKeyView (NSView child)
		{
			if (child == FirstKeyView || child == LastKeyView) {
				UpdateKeyViews ();
			}
		}

		public void UpdateKeyViews (bool backward = true, bool forward = true)
		{
			if (TableRow < 0)
				return;

			PropertyEditorControl ctrl = null;

			//FIXME: don't hardcode column
			if (backward && TableRow > 0 && (ctrl = TableView.GetView (1, TableRow - 1, false) as PropertyEditorControl) != null) {
				ctrl.LastKeyView.NextKeyView = FirstKeyView;
				ctrl.UpdateKeyViews (forward: false);
			}

			//FIXME: don't hardcode column
			if (forward && TableRow < TableView.RowCount - 1 && (ctrl = TableView.GetView (1, TableRow + 1, false) as PropertyEditorControl) != null) {
				LastKeyView.NextKeyView = ctrl.FirstKeyView;
				ctrl.UpdateKeyViews (backward: false);
			}
		}

		protected abstract void UpdateValue ();

		protected virtual void OnViewModelChanged (PropertyViewModel oldModel)
		{
			SetEnabled ();
			UpdateValue ();
			UpdateAccessibilityValues ();
		}

		protected virtual void OnPropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Value") {
				UpdateValue ();
			}
		}

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
