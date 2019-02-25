using System;
using AppKit;
using CoreGraphics;
using Foundation;
using Xamarin.PropertyEditing.ViewModels;

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

			AddConstraints (new[] {
				NSLayoutConstraint.Create (this.label, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this, NSLayoutAttribute.Right, Mac.Layout.GoldenRatioLeft, 0f),
				NSLayoutConstraint.Create (this.label, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 18),
			});

			if (EditorView != null) {
				AddSubview (EditorView.NativeView);
				EditorView.NativeView.TranslatesAutoresizingMaskIntoConstraints = false;

				if (EditorView.NativeView is PropertyEditorControl pec && pec.FirstKeyView != null) {
					AddConstraint (NSLayoutConstraint.Create (this.label, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, pec.FirstKeyView, NSLayoutAttribute.CenterY, 1, 0));
				} else {
					AddConstraint (NSLayoutConstraint.Create (this.label, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this, NSLayoutAttribute.Top, 1, 3));
				}

				AddConstraints (new[] {
					NSLayoutConstraint.Create (EditorView.NativeView, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, this, NSLayoutAttribute.CenterY, 1f, 0f),
					NSLayoutConstraint.Create (EditorView.NativeView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this.label, NSLayoutAttribute.Right, 1f, LabelToControlSpacing),
					NSLayoutConstraint.Create (EditorView.NativeView, NSLayoutAttribute.Height, NSLayoutRelation.Equal, this, NSLayoutAttribute.Height, 1f, 0)
				});

				if (editorView.NeedsPropertyButton) {
					this.propertyButton = new PropertyButton (hostResources) {
						TranslatesAutoresizingMaskIntoConstraints = false
					};

					AddSubview (this.propertyButton);
					AddConstraints (new[] {
						NSLayoutConstraint.Create (this.propertyButton, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, this.label, NSLayoutAttribute.CenterY, 1, 0),
						NSLayoutConstraint.Create (EditorView.NativeView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this.propertyButton, NSLayoutAttribute.Left, 1f, -EditorToButtonSpacing),
						NSLayoutConstraint.Create (this.propertyButton, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this, NSLayoutAttribute.Right, 1f, -ButtonToWallSpacing),
						NSLayoutConstraint.Create (this.propertyButton, NSLayoutAttribute.Width, NSLayoutRelation.Equal, 1f, PropertyButton.DefaultSize),
					});
				} else {
					AddConstraint (NSLayoutConstraint.Create (EditorView.NativeView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this, NSLayoutAttribute.Right, 1f, 0f));
				}
			} else {
				AddConstraint (NSLayoutConstraint.Create (this.label, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, this, NSLayoutAttribute.CenterY, 1, 0));
			}
		}

		public IEditorView EditorView {
			get;
		}

		public EditorViewModel ViewModel
		{
			get => EditorView?.ViewModel;
			set
			{
				if (EditorView == null)
					return;

				EditorView.ViewModel = value;
				this.propertyButton.ViewModel = value as PropertyViewModel;
			}
		}

		public string Label {
			get { return this.label.StringValue; }
			set {
				this.label.StringValue = value + ":";
				this.label.ToolTip = value;
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

		private UnfocusableTextField label = new UnfocusableTextField {
			Alignment = NSTextAlignment.Right,
			Cell = {
				LineBreakMode = NSLineBreakMode.TruncatingHead,
			},
			TranslatesAutoresizingMaskIntoConstraints = false,
		};

#if DEBUG // Currently only used to highlight which controls haven't been implemented
		public NSColor LabelTextColor {
			set { this.label.TextColor = value; }
		}
#endif


		internal const float LabelToControlSpacing = 13f;
		internal static float PropertyTotalWidth => PropertyButton.DefaultSize + ButtonToWallSpacing + EditorToButtonSpacing;

		private NSView leftEdgeView;
		private NSLayoutConstraint leftEdgeLeftConstraint, leftEdgeVCenterConstraint;
		private readonly IHostResourceProvider hostResources;
		private readonly PropertyButton propertyButton;

		private const float EditorToButtonSpacing = 4f;
		private const float ButtonToWallSpacing = 9f;
	}
}
