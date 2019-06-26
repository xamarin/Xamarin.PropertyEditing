using AppKit;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class EditorContainer
		: PropertyContainer
	{
		public EditorContainer (IHostResourceProvider hostResources, IEditorView editorView)
			: base (hostResources, editorView, editorView?.NeedsPropertyButton ?? false)
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

				if (EditorView.NeedsPropertyButton)
					PropertyButton.ViewModel = value as PropertyViewModel;
			}
		}

		public NSView LeftEdgeView
		{
			get { return this.leftEdgeView; }
			set
			{
				if (this.leftEdgeView != null) {
					this.leftEdgeView.RemoveFromSuperview ();
					RemoveConstraints (new[] { this.leftEdgeLeftConstraint, this.leftEdgeVCenterConstraint });
					this.leftEdgeLeftConstraint.Dispose ();
					this.leftEdgeLeftConstraint = null;
					this.leftEdgeVCenterConstraint.Dispose ();
					this.leftEdgeVCenterConstraint = null;
				}

				this.leftEdgeView = value;

				if (value != null) {
					AddSubview (value);

					value.TranslatesAutoresizingMaskIntoConstraints = false;
					this.leftEdgeLeftConstraint = NSLayoutConstraint.Create (this.leftEdgeView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this, NSLayoutAttribute.Left, 1, 4);
					this.leftEdgeVCenterConstraint = NSLayoutConstraint.Create (this.leftEdgeView, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, this, NSLayoutAttribute.CenterY, 1, 0);

					AddConstraints (new[] { this.leftEdgeLeftConstraint, this.leftEdgeVCenterConstraint });
				}
			}
		}

		public override void ViewWillMoveToSuperview (NSView newSuperview)
		{
			if (newSuperview == null && EditorView != null)
				EditorView.ViewModel = null;

			base.ViewWillMoveToSuperview (newSuperview);
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
