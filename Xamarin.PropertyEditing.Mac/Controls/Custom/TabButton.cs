using System;
using AppKit;
using CoreGraphics;
using Foundation;

namespace Xamarin.PropertyEditing.Mac
{
	internal interface IUnderliningTabView
	{
		event EventHandler Clicked;

		bool Selected { get; set; }
		int LineWidth { get; set; }
	}

	internal class TabButton
		: FocusableButton, IUnderliningTabView
	{
		public TabButton (IHostResourceProvider hostResource, string imageName = null)
		{
			if (hostResource == null)
				throw new ArgumentNullException (nameof (hostResource));

			Bordered = false;
			Action = new ObjCRuntime.Selector (ClickedName);

			var clicked = new NSClickGestureRecognizer (OnClicked);
			AddGestureRecognizer (clicked);

			this.hostResource = hostResource;
			this.imageName = imageName;

			AppearanceChanged ();
		}

		public event EventHandler Clicked;

		public bool Selected {
			get => this.selected;
			set {
				if (this.selected == value)
					return;

				this.selected = value;

				TitleColor = this.selected ? NSColor.Text : NSColor.DisabledControlText;
				NeedsDisplay = true;
			}
		}

		private int lineWidth = 2;
		public int LineWidth {
			get => this.lineWidth;
			set {
				if (this.lineWidth == value)
					return;

				this.lineWidth = value;
				NeedsLayout = true;
			}
		}

		private NSColor titleColor;
		protected NSColor TitleColor {
			get => this.titleColor;
			private set {
				if (this.titleColor == value)
					return;

				this.titleColor = value;

				// No point changing the text color if there's nothing to change.
				if (!string.IsNullOrEmpty (Title)) {
					var coloredTitle = new NSMutableAttributedString (Title);
					var titleRange = new NSRange (0, coloredTitle.Length);
					coloredTitle.AddAttribute (NSStringAttributeKey.ForegroundColor, this.titleColor, titleRange);
					var centeredAttribute = new NSMutableParagraphStyle ();
					centeredAttribute.Alignment = NSTextAlignment.Center;
					coloredTitle.AddAttribute (NSStringAttributeKey.ParagraphStyle, centeredAttribute, titleRange);
					AttributedTitle = coloredTitle;
				}
			}
		}

		public override CGSize IntrinsicContentSize {
			get {
				var size = base.IntrinsicContentSize;
				return new CGSize (size.Width + 2 + 10, size.Height + 2 + 10);
			}
		}

		public sealed override void ViewDidChangeEffectiveAppearance ()
		{
			base.ViewDidChangeEffectiveAppearance ();

			AppearanceChanged ();
		}

		public override void DrawRect (CGRect dirtyRect)
		{
			base.DrawRect (dirtyRect);
			if (!Selected)
				return;

			NSBezierPath path = new NSBezierPath ();
			path.AppendPathWithRect (new CGRect (Bounds.X, Bounds.Height - this.lineWidth, Bounds.Width, this.lineWidth));
			this.titleColor.Set ();
			path.Fill ();
		}

		private void AppearanceChanged ()
		{
			if (!string.IsNullOrEmpty (this.imageName))
				Image = this.hostResource.GetNamedImage (this.imageName);
		}

		private readonly string imageName;
		private readonly IHostResourceProvider hostResource;
		private bool selected;

		private const string ClickedName = "OnClicked";

		[Export (ClickedName)]
		private void OnClicked()
		{
			Clicked?.Invoke (this, EventArgs.Empty);
		}
	}
}