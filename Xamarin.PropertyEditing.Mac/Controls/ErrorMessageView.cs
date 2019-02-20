using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AppKit;
using CoreGraphics;

namespace Xamarin.PropertyEditing.Mac
{
	internal class ErrorMessageView : NSView
	{
		const int DefaultIconButtonSize = 24;

		private NSTextField errorMessages;

		public ErrorMessageView (IHostResourceProvider hostResources, IEnumerable errors)
		{
			if (errors == null)
				throw new ArgumentNullException (nameof (errors));

			Frame = new CGRect (CGPoint.Empty, new CGSize (320, 240));

			var iconView = new NSButton (new CGRect (5, Frame.Height - 25, DefaultIconButtonSize, DefaultIconButtonSize)) {
				Bordered = false,
				Image = hostResources.GetNamedImage ("pe-action-warning-16"),
				Title = string.Empty,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			AddSubview (iconView);

			var viewTitle = new UnfocusableTextField (new CGRect (30, Frame.Height - 26, 120, 24), "Errors");

			AddSubview (viewTitle);

			this.errorMessages = new NSTextField {
				BackgroundColor = NSColor.Clear,
				Editable = false,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};
			this.errorMessages.Cell.Wraps = true;

			foreach (var error in errors) {
				this.errorMessages.StringValue += error + "\n";
			}

			AddSubview (this.errorMessages);

			this.AddConstraints (new[] {
				NSLayoutConstraint.Create (iconView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Top, 1f, 5f),
				NSLayoutConstraint.Create (iconView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Left, 1f, 5f),
				NSLayoutConstraint.Create (iconView, NSLayoutAttribute.Width, NSLayoutRelation.Equal, 1f, DefaultIconButtonSize),
				NSLayoutConstraint.Create (iconView, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, DefaultIconButtonSize),

				NSLayoutConstraint.Create (viewTitle, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Top, 1f, 7f),
				NSLayoutConstraint.Create (viewTitle, NSLayoutAttribute.Left, NSLayoutRelation.Equal, iconView,  NSLayoutAttribute.Right, 1f, 5f),
				NSLayoutConstraint.Create (viewTitle, NSLayoutAttribute.Width, NSLayoutRelation.Equal, 1f, 120),
				NSLayoutConstraint.Create (viewTitle, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, PropertyEditorControl.DefaultControlHeight),

				NSLayoutConstraint.Create (this.errorMessages, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Top, 1f, 35f),
				NSLayoutConstraint.Create (this.errorMessages, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Left, 1f, 5f),
				NSLayoutConstraint.Create (this.errorMessages, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Width, 1f, -10f),
				NSLayoutConstraint.Create (this.errorMessages, NSLayoutAttribute.Height, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Height, 1f, -40f),
			});
		}
	}
}
