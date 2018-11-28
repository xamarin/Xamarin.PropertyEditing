using System;
using AppKit;
using CoreGraphics;

namespace Xamarin.PropertyEditing.Mac
{
	internal class BasePopOverControl : NSView
	{
		const int DefaultIconButtonSize = 32;

		public BasePopOverControl (string title, string imageNamed) : base ()
		{
			if (title == null)
				throw new ArgumentNullException (nameof (title));
			if (imageNamed == null)
				throw new ArgumentNullException (nameof (imageNamed));

			TranslatesAutoresizingMaskIntoConstraints = false;
			WantsLayer = true;

			var iconView = new NSImageView {
				Image = PropertyEditorPanel.ThemeManager.GetImageForTheme (imageNamed),
				ImageScaling = NSImageScale.None,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			AddSubview (iconView);

			var viewTitle = new UnfocusableTextField {
				Font = NSFont.BoldSystemFontOfSize (11),
				StringValue = title,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			AddSubview (viewTitle);

			this.DoConstraints (new[] {
				iconView.ConstraintTo (this, (iv, c) => iv.Top == c.Top + 5),
				iconView.ConstraintTo (this, (iv, c) => iv.Left == c.Left + 5),
				iconView.ConstraintTo (this, (iv, c) => iv.Width == DefaultIconButtonSize),
				iconView.ConstraintTo (this, (iv, c) => iv.Height == DefaultIconButtonSize),

				viewTitle.ConstraintTo (this, (vt, c) => vt.Top == c.Top + 8),
				viewTitle.ConstraintTo (this, (vt, c) => vt.Left == c.Left + 38),
				viewTitle.ConstraintTo (this, (vt, c) => vt.Width == 120),
				viewTitle.ConstraintTo (this, (vt, c) => vt.Height == 24),
			});

			Appearance = PropertyEditorPanel.ThemeManager.CurrentAppearance;
		}
	}
}
