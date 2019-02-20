using System;
using System.Collections;

using AppKit;
using CoreGraphics;

using Xamarin.PropertyEditing.Mac.Resources;

namespace Xamarin.PropertyEditing.Mac
{
	internal abstract class BaseEditorControl : NSView
	{
		private IEnumerable errorList;
		private const int DefaultActioButtonSize = 16;

		public event EventHandler ActionButtonClicked;

		private NSButton actionButton;
		public NSButton ActionButton => this.actionButton;

		private PropertyButton propertyButton;
		public PropertyButton PropertyButton => this.propertyButton;


		public BaseEditorControl (IHostResourceProvider hostResources)
		{
			if (hostResources == null)
				throw new ArgumentNullException (nameof (hostResources));

			HostResources = hostResources;

			this.propertyButton = new PropertyButton (hostResources) {
				TranslatesAutoresizingMaskIntoConstraints = false
			};

			AddSubview (this.propertyButton);

			this.actionButton = new NSButton {
				Bordered = false,
				Enabled = false,
				ImageScaling = NSImageScale.AxesIndependently,
				Title = string.Empty,
				TranslatesAutoresizingMaskIntoConstraints = false,
				AccessibilityTitle = LocalizationResources.AccessibilityActionButton,
				AccessibilityHelp = LocalizationResources.AccessibilityActionButtonDescription,
			};

			this.actionButton.Activated += (object sender, EventArgs e) => {
				if (this.errorList != null) {
					var Container = new ErrorMessageView (HostResources, this.errorList);

					var errorMessagePopUp = new NSPopover {
						Behavior = NSPopoverBehavior.Semitransient,
						ContentViewController = new NSViewController (null, null) { View = Container },
					};

					errorMessagePopUp.Show (default (CGRect), this.actionButton, NSRectEdge.MinYEdge);
				}

				NotifyActionButtonClicked ();
			};

			AddSubview (actionButton);

			this.AddConstraints (new[] {
				NSLayoutConstraint.Create (this.propertyButton, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Top, 1f, 1f),
				NSLayoutConstraint.Create (this.propertyButton, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Right, 1f, -33f),
				NSLayoutConstraint.Create (this.propertyButton, NSLayoutAttribute.Width, NSLayoutRelation.Equal, 1f, PropertyButton.DefaultSize),
				NSLayoutConstraint.Create (this.propertyButton, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, PropertyButton.DefaultSize),

				NSLayoutConstraint.Create (this.actionButton, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Top, 1f, 3f), // TODO: Better centering based on the icon height
				NSLayoutConstraint.Create (this.actionButton, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this.propertyButton,  NSLayoutAttribute.Left, 1f, 20f),
				NSLayoutConstraint.Create (this.actionButton, NSLayoutAttribute.Width, NSLayoutRelation.Equal, 1f, DefaultActioButtonSize),
				NSLayoutConstraint.Create (this.actionButton, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, DefaultActioButtonSize),
			});
		}

		public override void ViewDidChangeEffectiveAppearance ()
		{
			this.actionButton.Image = this.actionButton.Enabled ? HostResources.GetNamedImage ("pe-action-warning-16") : null;
		}

		protected IHostResourceProvider HostResources
		{
			get;
		}

		protected void SetErrors (IEnumerable errors)
		{
			this.errorList = errors;

			this.actionButton.Enabled = errors != null;
			this.actionButton.Hidden = !this.actionButton.Enabled;

			// Using NSImageName.Caution for now, we can change this later at the designers behest
			this.actionButton.Image = this.actionButton.Enabled ? HostResources.GetNamedImage ("pe-action-warning-16") : null;
		}

		void NotifyActionButtonClicked ()
		{
			ActionButtonClicked?.Invoke (this, EventArgs.Empty);
		}
	}
}
