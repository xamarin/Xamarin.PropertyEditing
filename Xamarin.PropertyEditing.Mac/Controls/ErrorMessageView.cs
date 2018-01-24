using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AppKit;
using CoreGraphics;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class ErrorMessageView : NSView
	{
		const int DefaultIconButtonSize = 24;

		NSTextField ErrorMessages;

		public ErrorMessageView (IEnumerable errors)
		{
			if (errors == null)
				throw new ArgumentNullException (nameof (errors));

			Frame = new CGRect (CGPoint.Empty, new CGSize (320, 240));

			var iconView = new NSButton (new CGRect (5, Frame.Height - 25, DefaultIconButtonSize, DefaultIconButtonSize)) {
				Bordered = false,
				Image = NSImage.ImageNamed ("action-warning-16"),
				Title = string.Empty,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			AddSubview (iconView);

			var viewTitle = new UnfocusableTextField (new CGRect (30, Frame.Height - 26, 120, 24), "Errors");

			AddSubview (viewTitle);

			ErrorMessages = new NSTextField {
				BackgroundColor = NSColor.Clear,
				Editable = false,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			foreach (var error in errors) {
				ErrorMessages.StringValue += error + "\n";
			}

			AddSubview (ErrorMessages);

			this.DoConstraints (new[] {
				iconView.ConstraintTo (this, (iv, c) => iv.Top == c.Top + 5),
				iconView.ConstraintTo (this, (iv, c) => iv.Left == c.Left + 5),
				iconView.ConstraintTo (this, (iv, c) => iv.Width == DefaultIconButtonSize),
				iconView.ConstraintTo (this, (iv, c) => iv.Height == DefaultIconButtonSize),
				viewTitle.ConstraintTo (this, (vt, c) => vt.Top == c.Top + 7),
				viewTitle.ConstraintTo (this, (vt, c) => vt.Width == 120),
				viewTitle.ConstraintTo (this, (vt, c) => vt.Height == 24),
				ErrorMessages.ConstraintTo (this, (s, c) => s.Top == c.Top + 35),
				ErrorMessages.ConstraintTo (this, (s, c) => s.Left == c.Left + 5),
				ErrorMessages.ConstraintTo (this, (s, c) => s.Width == c.Width - 10),
				ErrorMessages.ConstraintTo (this, (s, c) => s.Height == c.Height - 40),
			});

			this.Appearance = PropertyEditorPanel.ThemeManager.CurrentAppearance;
		}
	}
}
