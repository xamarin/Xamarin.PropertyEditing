using System;
using System.Collections;
using System.Collections.Generic;
using AppKit;
using CoreGraphics;

namespace Xamarin.PropertyEditing.Mac
{
	internal class BaseEditorControl : NSView
	{
		NSButton errorButton;
		IEnumerable errorList;

		public BaseEditorControl ()
		{
			errorButton = new NSButton () {
				Bordered = false,
				Cell = { BackgroundColor = NSColor.Clear },
				Enabled = false,
				Title = string.Empty,
				TranslatesAutoresizingMaskIntoConstraints = false,
				ImageScaling = NSImageScale.AxesIndependently,
			};

			errorButton.Activated += (object sender, EventArgs e) => {
				var Container = new ErrorMessageView (errorList) {
					Frame = new CGRect (CGPoint.Empty, new CGSize (320, 200))
				};

				var errorMessagePopUp = new NSPopover {
					Behavior = NSPopoverBehavior.Semitransient,
					ContentViewController = new NSViewController (null, null) { View = Container },
				};

				errorMessagePopUp.Show (default (CGRect), errorButton, NSRectEdge.MinYEdge);
			};

			AddSubview (errorButton);

			this.DoConstraints (new[] {
				errorButton.ConstraintTo (this, (cb, c) => cb.Width == 20),
				errorButton.ConstraintTo (this, (cb, c) => cb.Height == 20),
				errorButton.ConstraintTo (this, (cb, c) => cb.Left == c.Right - 22),
				errorButton.ConstraintTo (this, (cb, c) => cb.Top == c.Top + 4),
			});

			PropertyEditorPanel.ThemeManager.ThemeChanged += (object sender, EventArgs e) => {
				UpdateTheme ();
			};
		}

		protected void UpdateTheme ()
		{
			this.Appearance = PropertyEditorPanel.ThemeManager.CurrentAppearance;
		}

		protected void SetErrors (IEnumerable errors)
		{
			errorList = errors;

			errorButton.Enabled = errors != null;
			// Using NSImageName.Caution for now, we can change this later at the designers behest
			errorButton.Image = errorButton.Enabled ? NSImage.ImageNamed (NSImageName.Caution) : null;
		}
	}
}
