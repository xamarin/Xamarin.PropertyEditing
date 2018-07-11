using AppKit;
using CoreGraphics;

namespace Xamarin.PropertyEditing.Mac
{
	internal class UnderlinedTextField : NSTextField
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
				//if (selected == value)
				//	return;
				selected = value;
				TextColor = selected ? NSColor.Text : NSColor.DisabledControlText;

				//Enabled = value;
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
