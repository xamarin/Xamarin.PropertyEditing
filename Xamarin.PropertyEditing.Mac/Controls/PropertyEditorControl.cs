using System;
using System.Collections;
using AppKit;
using CoreGraphics;
using Foundation;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal abstract class PropertyEditorControl
		: BaseEditorControl, IEditorView
	{
		protected PropertyEditorControl (IHostResourceProvider hostResources)
			: base (hostResources)
		{
		}

		public string Label { get; set; }

		public abstract NSView FirstKeyView { get; }
		public abstract NSView LastKeyView { get; }

		public NSTableView TableView { get; set; }

		public const int DefaultControlHeight = 22;
		public const int DefaultFontSize = 11;
		public const int DefaultPropertyLabelFontSize = 11;
		public const int DefaultDescriptionLabelFontSize = 10;
		public const string DefaultFontName = ".AppleSystemUIFont";
		public virtual bool IsDynamicallySized => false;

		PropertyViewModel viewModel;
		public PropertyViewModel ViewModel {
			get { return this.viewModel; }
			set {
				if (this.ViewModel == value)
					return;

				PropertyViewModel oldModel = this.viewModel;
				if (oldModel != null) {
					oldModel.PropertyChanged -= OnPropertyChanged;
					oldModel.ErrorsChanged -= HandleErrorsChanged;
				}

				this.viewModel = value;
				OnViewModelChanged (oldModel);
				if (this.viewModel != null) {
					this.viewModel.PropertyChanged += OnPropertyChanged;
					this.viewModel.ErrorsChanged += HandleErrorsChanged;
				}
			}
		}

		EditorViewModel IEditorView.ViewModel
		{
			get { return this.ViewModel; }
			set { ViewModel = (PropertyViewModel)value; }
		}

		NSView IEditorView.NativeView => this;

		[Export ("_primitiveSetDefaultNextKeyView:")]
		public void SetDefaultNextKeyView (NSView child)
		{
			if (child == FirstKeyView || child == LastKeyView) {
				UpdateKeyViews ();
			}
		}

		public void UpdateKeyViews ()
		{
			nint row = TableView.RowForView (this);
			if (row <= 0)
				return;

			NSView view;
			PropertyEditorControl ctrl = null;
			do {
				row--;
				view = TableView.GetView (0, row, makeIfNecessary: false);
				ctrl = (view as EditorContainer)?.EditorView?.NativeView as PropertyEditorControl;
			} while (row > 0 && ctrl == null);

			if (ctrl != null) {
				ctrl.LastKeyView.NextKeyView = FirstKeyView;
				ctrl.UpdateKeyViews ();
			} else if (row == 0 && view is PanelHeaderEditorControl header) {
				header.SetNextKeyView (FirstKeyView);
			}
		}

		/// <remarks>You should treat the implementation of this as static.</remarks>
		public virtual nint GetHeight (EditorViewModel vm)
		{
			return DefaultControlHeight;
		}

		protected abstract void UpdateValue ();

		protected virtual void OnViewModelChanged (PropertyViewModel oldModel)
		{
			if (ViewModel != null) {
				SetEnabled ();
				UpdateValue ();
				UpdateAccessibilityValues ();
			}

			PropertyButton.ViewModel = viewModel;
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

	internal abstract class PropertyEditorControl<TViewModel>
		: PropertyEditorControl
		where TViewModel : PropertyViewModel
	{
		public PropertyEditorControl (IHostResourceProvider hostResources)
			: base (hostResources)
		{
		}

		internal new TViewModel ViewModel
		{
			get { return (TViewModel)base.ViewModel; }
			set { base.ViewModel = value; }
		}
	}
}
