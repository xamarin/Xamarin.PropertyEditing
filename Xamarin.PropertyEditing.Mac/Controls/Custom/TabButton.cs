using System;
using AppKit;
using CoreGraphics;
using Foundation;

namespace Xamarin.PropertyEditing.Mac
{
	internal class TabButton
		: NSButton
	{
		public TabButton (IHostResourceProvider hostResource, string imageName)
		{
			if (hostResource == null)
				throw new ArgumentNullException (nameof (hostResource));
			if (imageName == null)
				throw new ArgumentNullException (nameof (imageName));

			Bordered = false;
			Action = new ObjCRuntime.Selector (ClickedName);

			var clicked = new NSClickGestureRecognizer (OnClicked);
			AddGestureRecognizer (clicked);

			this.hostResource = hostResource;
			this.imageName = imageName;

			ViewDidChangeEffectiveAppearance ();
		}

		public event EventHandler Clicked;

		public bool Selected
		{
			get { return this.selected; }
			set
			{
				this.selected = value;
				NeedsDisplay = true;
			}
		}

		public override CGSize IntrinsicContentSize
		{
			get
			{
				var size = base.IntrinsicContentSize;
				return new CGSize (size.Width + 2 + 10, size.Height + 2 + 10);
			}
		}

		public override void ViewDidChangeEffectiveAppearance ()
		{
			base.ViewDidChangeEffectiveAppearance ();
			Image = this.hostResource.GetNamedImage (this.imageName);
		}

		public override void DrawRect (CGRect dirtyRect)
		{
			base.DrawRect (dirtyRect);
			if (!Selected)
				return;

			NSBezierPath path = new NSBezierPath ();
			path.AppendPathWithRect (new CGRect (Bounds.X, Bounds.Height - 2, Bounds.Width, 2));
			(Selected ? NSColor.Text : NSColor.DisabledControlText).Set ();
			path.Fill ();
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
