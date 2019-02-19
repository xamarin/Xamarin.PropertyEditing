using System;
using AppKit;

namespace Xamarin.PropertyEditing.Mac
{
	internal class EditorContainer
		: NSView
	{
		public EditorContainer (IHostResourceProvider hostResources, IEditorView editorView)
		{
			if (hostResources == null)
				throw new ArgumentNullException (nameof (hostResources));

			EditorView = editorView;

			AddSubview (this.label);

			this.AddConstraints (new[] {
				NSLayoutConstraint.Create (this.label, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this, NSLayoutAttribute.Top, 1f, 0f),
				NSLayoutConstraint.Create (this.label, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this, NSLayoutAttribute.Right, Mac.Layout.GoldenRatioLeft, 0f),
				NSLayoutConstraint.Create (this.label, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, PropertyEditorControl.DefaultControlHeight),
			});

			if (EditorView != null) {
				AddSubview (EditorView.NativeView);
				EditorView.NativeView.TranslatesAutoresizingMaskIntoConstraints = false;

				this.AddConstraints (new[] {
					NSLayoutConstraint.Create (EditorView.NativeView, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, this, NSLayoutAttribute.CenterY, 1f, 0f),
					NSLayoutConstraint.Create (EditorView.NativeView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this.label, NSLayoutAttribute.Right, 1f, 5f),
					NSLayoutConstraint.Create (EditorView.NativeView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this, NSLayoutAttribute.Right, 1f, 0f),
					NSLayoutConstraint.Create (EditorView.NativeView, NSLayoutAttribute.Height, NSLayoutRelation.Equal, this, NSLayoutAttribute.Height, 1f, 0f)
				});
			}
		}

		public IEditorView EditorView {
			get;
		}

		public string Label {
			get { return this.label.StringValue; }
			set { this.label.StringValue = value; }
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

		private UnfocusableTextField label = new UnfocusableTextField {
			Alignment = NSTextAlignment.Right,
			TranslatesAutoresizingMaskIntoConstraints = false
		};

#if DEBUG // Currently only used to highlight which controls haven't been implemented
		public NSColor LabelTextColor {
			set { this.label.TextColor = value; }
		}
#endif

		private NSView leftEdgeView;
		private NSLayoutConstraint leftEdgeLeftConstraint, leftEdgeVCenterConstraint;
	}
}
