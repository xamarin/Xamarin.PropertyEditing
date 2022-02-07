using System;
using AppKit;
using CoreAnimation;
using CoreGraphics;
using ObjCRuntime;

namespace Xamarin.PropertyEditing.Mac
{
	internal class HeaderView : NSView
	{
		public string Title {
			get { return this.headerText.StringValue; }
			set { this.headerText.StringValue = value; }
		}

		private NSLayoutConstraint horizonalHeaderTextAlignment;
		public nfloat HorizonalTitleOffset {
			get { return this.horizonalHeaderTextAlignment.Constant; }
			set { this.horizonalHeaderTextAlignment.Constant = value; }
		}

        private UnfocusableTextField headerText = new UnfocusableTextField {
			TranslatesAutoresizingMaskIntoConstraints = false,
		};

		internal HeaderView ()
		{
			TranslatesAutoresizingMaskIntoConstraints = false;
			WantsLayer = true;

			// Layer out of alphabetical order so that WantsLayer creates the layer first
			Layer = new CALayer {
				CornerRadius = 1.0f,
				BorderColor = new CGColor (.5f, .5f, .5f, 1.0f),
				BorderWidth = 1,
			};

			this.horizonalHeaderTextAlignment = NSLayoutConstraint.Create (this.headerText, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, this, NSLayoutAttribute.CenterX, 1, 0);

			AddSubview (this.headerText);
			AddConstraints (new[] {
				NSLayoutConstraint.Create (this.headerText, NSLayoutAttribute.Height, NSLayoutRelation.Equal, this, NSLayoutAttribute.Height, 1f, 0f),
				this.horizonalHeaderTextAlignment,
				NSLayoutConstraint.Create (this.headerText, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, this, NSLayoutAttribute.CenterY, 1, 0f),
			});
		}
	}
}
