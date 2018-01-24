using System;
using AppKit;
using CoreGraphics;

namespace Xamarin.PropertyEditing.Mac
{
	internal class BasePopOverControl : NSView
	{
		const int DefaultIconButtonSize = 24;

		public BasePopOverControl (string title, string imageNamed)
		{
			if (title == null)
				throw new ArgumentNullException (nameof (title));
			if (imageNamed == null)
				throw new ArgumentNullException (nameof (imageNamed));

			Frame = new CGRect (CGPoint.Empty, new CGSize (320, 180));

			var iconView = new NSButton (new CGRect (5, Frame.Height - 25, DefaultIconButtonSize, DefaultIconButtonSize)) {
				Bordered = false,
				Image = NSImage.ImageNamed (imageNamed),
				Title = string.Empty,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			AddSubview (iconView);

			var viewTitle = new UnfocusableTextField (new CGRect (30, Frame.Height - 26, 120, 24), title);

			AddSubview (viewTitle);

			this.DoConstraints (new[] {
				iconView.ConstraintTo (this, (iv, c) => iv.Top == c.Top + 5),
				iconView.ConstraintTo (this, (iv, c) => iv.Left == c.Left + 5),
				iconView.ConstraintTo (this, (iv, c) => iv.Width == DefaultIconButtonSize),
				iconView.ConstraintTo (this, (iv, c) => iv.Height == DefaultIconButtonSize),
				viewTitle.ConstraintTo (this, (vt, c) => vt.Top == c.Top + 7),
				viewTitle.ConstraintTo (this, (vt, c) => vt.Width == 120),
				viewTitle.ConstraintTo (this, (vt, c) => vt.Height == 24),
			});

			this.Appearance = PropertyEditorPanel.ThemeManager.CurrentAppearance;
		}
	}
}
