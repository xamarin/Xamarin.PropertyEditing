using AppKit;
using CoreGraphics;

namespace Xamarin.PropertyEditing.Mac
{
	internal interface IUnderliningTabView {
		bool Selected { get; set; }
		int LineWidth { get; set; }
	}

	internal class UnderlinedImageView : NSImageView, IUnderliningTabView
	{
		public UnderlinedImageView (string name)
		{
			this.name = name;
		}

		private string name;

		private bool selected;
		public bool Selected
		{
			get => selected;
			set {
				if (selected == value && Image != null)
					return;

				selected = value;
				NeedsDisplay = true;
			}
		}

		private int lineWidth = 2;
		public int LineWidth
		{
			get => lineWidth;
			set {
				if (this.lineWidth == value)
					return;

				this.lineWidth = value;
				NeedsLayout = true;
			}
		}


		private string VersionName {
			get {
				return PropertyEditorPanel.ThemeManager.GetImageNameForTheme (this.name, this.selected);
			}
		}

		public override void DrawRect (CGRect dirtyRect)
		{
			if (Image?.Name != VersionName) {
				Image = PropertyEditorPanel.ThemeManager.GetImageForTheme (this.name, this.selected);
			}

			base.DrawRect (dirtyRect);
			if (!Selected)
				return;
			
			NSBezierPath path = new NSBezierPath ();
			path.AppendPathWithRect (new CGRect (Bounds.X, Bounds.Top + this.lineWidth, Bounds.Width, this.lineWidth));
			(selected? NSColor.Text: NSColor.DisabledControlText).Set ();
			path.Fill ();
		}

		public override CGSize IntrinsicContentSize
		{
			get {
				var size = base.IntrinsicContentSize;
				return new CGSize (size.Width + this.lineWidth + 10, size.Height + this.lineWidth + 10);
			}
		}
	}

	internal class UnderlinedTextField : NSTextField, IUnderliningTabView
	{
		public UnderlinedTextField ()
		{
			TranslatesAutoresizingMaskIntoConstraints = false;
			AutoresizingMask = NSViewResizingMask.MaxXMargin;
			UsesSingleLineMode = true;
			Alignment = NSTextAlignment.Center;
		}

		private bool selected;
		public bool Selected
		{
			get => this.selected;
			set
			{
				this.selected = value;
				TextColor = this.selected ? NSColor.Text : NSColor.DisabledControlText;
               
				///NeedsDisplay = true;
			}
		}

		private int lineWidth = 2;
		public int LineWidth {
			get =>this.lineWidth;
			set {
				if (this.lineWidth == value)
					return;

				this.lineWidth = value;
				NeedsLayout = true;
			}
		}

		public override void DrawRect (CGRect dirtyRect)
		{
			base.DrawRect (dirtyRect);
			if (!Selected)
				return;

			NSBezierPath path = new NSBezierPath ();
			path.AppendPathWithRect (new CGRect (Bounds.X, Bounds.Bottom - lineWidth, Bounds.Width, lineWidth));
			TextColor.Set ();
			path.Fill ();
		}

		public override CGSize IntrinsicContentSize
		{
			get
			{
				var size = base.IntrinsicContentSize;
				return new CGSize (size.Width, size.Height + lineWidth + 3);
			}
		}
	}
}
