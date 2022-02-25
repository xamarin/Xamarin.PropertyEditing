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
		public const float DefaultButtonWidth = 70f;
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


		public virtual bool NeedsPropertyButton => true;


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

		protected virtual void AppearanceChanged ()
		{
		}

		public sealed override void ViewDidChangeEffectiveAppearance ()
		{
			base.ViewDidChangeEffectiveAppearance ();

			AppearanceChanged ();
		}

		public void OnNextResponderRequested (bool reverse)
		{
			if (TableView != null) {
				var modifier = reverse ? -1 : 1;

				nint row = TableView.RowForView (this) + modifier;

				NSView view;
				PropertyEditorControl ctrl = null;

				var rowCount = TableView.RowCount;
				for (; reverse ? row > 0 : row < rowCount; row += modifier) {

					view = TableView.GetView (0, row, makeIfNecessary: false);
					if (view is PropertyEditorControl pec) { // This is to include the CategoryContainer
						ctrl = pec;
					} else {
						ctrl = (view as EditorContainer)?.EditorView?.NativeView as PropertyEditorControl;
					}

					if (ctrl?.viewModel != null && !ctrl.viewModel.IsInputEnabled) {
						ctrl = null;
					}

					if (ctrl != null) {
						var targetView = reverse ? ctrl.LastKeyView : ctrl.FirstKeyView;
						Window?.MakeFirstResponder (targetView);
						return;
					} else if (row == 0 && view is PanelHeaderEditorControl header) {
						Window?.MakeFirstResponder (header);
						return;
					}
				}
			}
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
