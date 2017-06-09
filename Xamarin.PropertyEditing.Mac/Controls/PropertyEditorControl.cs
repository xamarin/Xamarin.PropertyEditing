using System;
using System.Collections;
using System.Runtime.InteropServices;

using AppKit;
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	public abstract class PropertyEditorControl : NSView
	{
		public string Label { get; set; }

		public abstract NSView FirstKeyView { get; }
		public abstract NSView LastKeyView { get; }

		public nint TableRow { get; set; }
		public NSTableView TableView { get; set; }

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
		}

		[Export ("_primitiveSetDefaultNextKeyView:")]
		public void SetDefaultNextKeyView (NSView child)
		{
			if (child == FirstKeyView || child == LastKeyView) {
				UpdateKeyViews ();
			} else {
				//FIXME: Unfortunately, XamMac doesn't provide an easier way to call super on a selector
				//  we manually declare with [Export]
				objc_msgSendSuper (SuperHandle, Selector.GetHandle ("_primitiveSetDefaultNextKeyView:"), child.Handle);
			}
		}

		public void UpdateKeyViews (bool backward = true, bool forward = true)
		{
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

		protected abstract void HandlePropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e);

		/// <summary>
		/// Update the display with any errors we need to show or remove
		/// </summary>
		/// <param name="errors">The error messages to display to the user</param>
		protected abstract void UpdateErrorsDisplayed (IEnumerable errors);

		protected abstract void HandleErrorsChanged (object sender, System.ComponentModel.DataErrorsChangedEventArgs e);

		protected abstract void SetEnabled ();

		[DllImport (Constants.ObjectiveCLibrary)]
		static extern void objc_msgSendSuper (IntPtr objc_super, IntPtr sel, IntPtr arg);
	}
}
