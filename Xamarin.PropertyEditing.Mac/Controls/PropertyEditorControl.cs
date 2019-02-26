using System;
using AppKit;
using Foundation;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal abstract class PropertyEditorControl
		: NSView, IEditorView
	{
		protected PropertyEditorControl (IHostResourceProvider hostResources)
		{
			if (hostResources == null)
				throw new ArgumentNullException (nameof (hostResources));

			HostResources = hostResources;
		}

		public IHostResourceProvider HostResources
		{
			get;
		}

		public string Label { get; set; }

		public abstract NSView FirstKeyView { get; }
		public abstract NSView LastKeyView { get; }

		public NSTableView TableView { get; set; }

		public const int DefaultControlHeight = 24;
		public const int DefaultFontSize = 11;
		public const int DefaultPropertyLabelFontSize = 11;
		public const int DefaultDescriptionLabelFontSize = 9;
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
				}

				this.viewModel = value;
				OnViewModelChanged (oldModel);
				if (this.viewModel != null) {
					this.viewModel.PropertyChanged += OnPropertyChanged;
				}
			}
		}

		EditorViewModel IEditorView.ViewModel
		{
			get { return this.ViewModel; }
			set { ViewModel = (PropertyViewModel)value; }
		}

		NSView INativeContainer.NativeView => this;

		[Export ("_primitiveSetDefaultNextKeyView:")]
		public void SetDefaultNextKeyView (NSView child)
		{
			if (child == FirstKeyView || child == LastKeyView) {
				UpdateKeyViews ();
			}
		}

		public virtual bool NeedsPropertyButton => true;

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
			return 24;
		}

		protected virtual void UpdateValue ()
		{
		}

		protected virtual void OnViewModelChanged (PropertyViewModel oldModel)
		{
			if (ViewModel != null) {
				SetEnabled ();
				UpdateValue ();
				UpdateAccessibilityValues ();
			}
		}

		protected virtual void OnPropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Value") {
				UpdateValue ();
			}
		}

		protected virtual void SetEnabled ()
		{
		}

		protected virtual void UpdateAccessibilityValues ()
		{
		}
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
