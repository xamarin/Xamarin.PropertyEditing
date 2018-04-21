using System;
using AppKit;
using CoreGraphics;

namespace Xamarin.PropertyEditing.Mac
{
	internal class BasePopOverControl : NSView
	{
		const int DefaultIconButtonSize = 24;

		public BasePopOverControl (string title, string imageNamed) : base ()
		{
			if (title == null)
				throw new ArgumentNullException (nameof (title));
			if (imageNamed == null)
				throw new ArgumentNullException (nameof (imageNamed));

			TranslatesAutoresizingMaskIntoConstraints = false;
			WantsLayer = true;

			var iconView = new NSImageView {
				Image = NSImage.ImageNamed (imageNamed),
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			AddSubview (iconView);

			var viewTitle = new UnfocusableTextField () {
				StringValue = title,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			AddSubview (viewTitle);

			this.DoConstraints (new[] {
				iconView.ConstraintTo (this, (iv, c) => iv.Top == c.Top + 5),
				iconView.ConstraintTo (this, (iv, c) => iv.Left == c.Left + 5),
				iconView.ConstraintTo (this, (iv, c) => iv.Width == DefaultIconButtonSize),
				iconView.ConstraintTo (this, (iv, c) => iv.Height == DefaultIconButtonSize),
				viewTitle.ConstraintTo (this, (vt, c) => vt.Top == c.Top + 6),
				viewTitle.ConstraintTo (this, (vt, c) => vt.Left == c.Left + 30),
				viewTitle.ConstraintTo (this, (vt, c) => vt.Width == 120),
				viewTitle.ConstraintTo (this, (vt, c) => vt.Height == 24),
			});

			this.Appearance = PropertyEditorPanel.ThemeManager.CurrentAppearance;
		}
	}
}
