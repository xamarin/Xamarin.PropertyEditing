using System;
using AppKit;

namespace Xamarin.PropertyEditing.Mac
{
	internal class EditorContainer
		: NSView
	{
		public EditorContainer (IEditorView editorView)
		{
			EditorView = editorView;

			AddSubview (this.label);
			this.AddConstraints (new[] {
				NSLayoutConstraint.Create (this.label, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, this, NSLayoutAttribute.CenterY, 1f, 0f),
				NSLayoutConstraint.Create (this.label, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this, NSLayoutAttribute.Right, Mac.Layout.GoldenRatioLeft, 0f),
				NSLayoutConstraint.Create (this.label, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this, NSLayoutAttribute.Left, 1f, 0f),
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

		public IEditorView EditorView
		{
			get;
		}

		public string Label
		{
			get { return this.label.StringValue; }
			set { this.label.StringValue = value; }
		}

		private UnfocusableTextField label = new UnfocusableTextField {
			Alignment = NSTextAlignment.Right,
			TranslatesAutoresizingMaskIntoConstraints = false
		};
	}
}
