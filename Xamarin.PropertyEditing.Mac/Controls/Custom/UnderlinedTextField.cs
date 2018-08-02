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
				if (lineWidth == value)
					return;

				lineWidth = value;
				NeedsLayout = true;
			}
		}


		private string VersionName {
			get {
				return PropertyEditorPanel.ThemeManager.GetImageNameForTheme (name, selected ? "~sel" : string.Empty);
			}
		}

		public override void DrawRect (CGRect dirtyRect)
		{
			if (Image?.Name != VersionName) {
				Image = PropertyEditorPanel.ThemeManager.GetImageForTheme (name, selected ? "~sel" : string.Empty);
			}

			base.DrawRect (dirtyRect);
			if (!Selected)
				return;
			
			NSBezierPath path = new NSBezierPath ();
			path.AppendPathWithRect (new CGRect (Bounds.X, Bounds.Top + lineWidth, Bounds.Width, lineWidth));
			(selected? NSColor.Text: NSColor.DisabledControlText).Set ();
			path.Fill ();
		}

		public override CGSize IntrinsicContentSize
		{
			get {
				var size = base.IntrinsicContentSize;
				return new CGSize (size.Width + lineWidth + 10, size.Height + lineWidth + 10);
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
			get => selected;
			set
			{
				selected = value;
				TextColor = selected ? NSColor.Text : NSColor.DisabledControlText;
               
				///NeedsDisplay = true;
			}
		}

		private int lineWidth = 2;
		public int LineWidth {
			get => lineWidth;
			set {
				if (lineWidth == value)
					return;

				lineWidth = value;
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
