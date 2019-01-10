using System;
using AppKit;
using CoreAnimation;
using CoreGraphics;

namespace Xamarin.PropertyEditing.Mac
{
	internal class HeaderView : NSView
	{
		public string Title
		{
			get { return this.headerText.StringValue; }
			set { this.headerText.StringValue = value; }
		}

		private UnfocusableTextField headerText = new UnfocusableTextField ();

		internal HeaderView ()
		{
			TranslatesAutoresizingMaskIntoConstraints = false;
			WantsLayer = true;

			// Layer out of alphabetical order so that WantsLayer creates the layer first
			Layer = new CALayer {
				CornerRadius = 1.0f,
				BorderColor = new CGColor (.5f, .5f, .5f, .5f),
				BorderWidth = 1,
			};

			this.headerText.Alignment = NSTextAlignment.Center;
			this.headerText.TranslatesAutoresizingMaskIntoConstraints = false;

			AddSubview (this.headerText);
			AddConstraints (new[] {
				NSLayoutConstraint.Create (this.headerText, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this, NSLayoutAttribute.Top, 1f, 0f),
				NSLayoutConstraint.Create (this.headerText, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this, NSLayoutAttribute.Left, 1f, 0f),
				NSLayoutConstraint.Create (this.headerText, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this, NSLayoutAttribute.Width, 1f, 0f),
				NSLayoutConstraint.Create (this.headerText, NSLayoutAttribute.Height, NSLayoutRelation.Equal, this, NSLayoutAttribute.Height, 1f, 0f),
			});
		}
	}
}
