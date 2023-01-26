using AppKit;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class EditorContainer
		: PropertyContainer
	{
		public EditorContainer (IHostResourceProvider hostResources, IEditorView editorView, bool? needsPropertyButton = null)
			: base (hostResources, editorView, needsPropertyButton ?? editorView?.NeedsPropertyButton ?? false)
		{
		}

		public IEditorView EditorView => (IEditorView)NativeContainer;

		public EditorViewModel ViewModel
		{
			get => EditorView?.ViewModel;
			set {
				if (EditorView == null)
					return;

				EditorView.ViewModel = value;

				if (PropertyButton != null)
					PropertyButton.ViewModel = value as PropertyViewModel;
			}
		}

#if DEBUG // Currently only used to highlight which controls haven't been implemented
		public NSColor LabelTextColor {
			set { LabelControl.TextColor = value; }
		}
#endif

		private NSView leftEdgeView;
		private NSLayoutConstraint leftEdgeLeftConstraint, leftEdgeVCenterConstraint;
	}
}
