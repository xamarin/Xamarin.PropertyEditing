using AppKit;
using CoreGraphics;

namespace Xamarin.PropertyEditing.Mac
{
	internal interface IUnderliningTabView {
		bool Selected { get; set; }
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

				var version = PropertyEditorPanel.ThemeManager.Theme == Themes.PropertyEditorTheme.Dark ? $"{name}~dark" : name;
				Image = NSImage.ImageNamed (selected ? $"{version}~sel" : version);

				NeedsDisplay = true;
			}
		}

		public override void DrawRect (CGRect dirtyRect)
		{
			base.DrawRect (dirtyRect);
			if (!Selected)
				return;
			
			NSBezierPath path = new NSBezierPath ();
			path.AppendPathWithRect (new CGRect (Bounds.X + 1, Bounds.Top + 3, Bounds.Width - 2, 3));
			(selected? NSColor.Text: NSColor.DisabledControlText).Set ();
			path.Fill ();
		}

		public override CGSize IntrinsicContentSize
		{
			get {
				var size = base.IntrinsicContentSize;
				return new CGSize (size.Width + 2, size.Height + 12);
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
		}

		private bool selected;
		public bool Selected
		{
			get => selected;
			set
			{
				selected = value;
				TextColor = selected ? NSColor.Text : NSColor.DisabledControlText;
               
				NeedsDisplay = true;
			}
		}

		public override void DrawRect (CGRect dirtyRect)
		{
			base.DrawRect (dirtyRect);
			if (!Selected)
				return;

			NSBezierPath path = new NSBezierPath ();
			path.AppendPathWithRect (new CGRect (Bounds.X + 1, Bounds.Bottom - 3, Bounds.Width - 2, 3.5));
			NSColor.Text.Set ();
			path.Fill ();
		}

		public override CGSize IntrinsicContentSize
		{
			get
			{
				var size = base.IntrinsicContentSize;
				return new CGSize (size.Width + 2, size.Height + 5);
			}
		}
	}
}
