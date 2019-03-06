using System;
using AppKit;
using CoreGraphics;

namespace Xamarin.PropertyEditing.Mac
{
	internal class BasePopOverControl : NSView
	{
		const int DefaultIconButtonSize = 32;

		public BasePopOverControl (IHostResourceProvider hostResources, string title, string imageNamed) : base ()
		{
			if (title == null)
				throw new ArgumentNullException (nameof (title));
			if (imageNamed == null)
				throw new ArgumentNullException (nameof (imageNamed));
			if (hostResources == null)
				throw new ArgumentNullException (nameof (hostResources));

			TranslatesAutoresizingMaskIntoConstraints = false;
			WantsLayer = true;

			HostResources = hostResources;

			var iconView = new NSImageView {
				Image = hostResources.GetNamedImage (imageNamed),
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

			this.AddConstraints (new[] {
				NSLayoutConstraint.Create (iconView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Top, 1f, 5f),
				NSLayoutConstraint.Create (iconView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Left, 1f, 5f),
				NSLayoutConstraint.Create (iconView, NSLayoutAttribute.Width, NSLayoutRelation.Equal, 1f, DefaultIconButtonSize),
				NSLayoutConstraint.Create (iconView, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, DefaultIconButtonSize),

				NSLayoutConstraint.Create (viewTitle, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Top, 1f, 7f),
				NSLayoutConstraint.Create (viewTitle, NSLayoutAttribute.Left, NSLayoutRelation.Equal, iconView,  NSLayoutAttribute.Right, 1f, 5f),
				//NSLayoutConstraint.Create (viewTitle, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Left, 1f, 38f),
				NSLayoutConstraint.Create (viewTitle, NSLayoutAttribute.Width, NSLayoutRelation.Equal, 1f, 120),
				NSLayoutConstraint.Create (viewTitle, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 24),
			});
		}

		protected IHostResourceProvider HostResources
		{
			get;
			private set;
		}
	}
}
