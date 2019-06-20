using System;
using AppKit;
using CoreGraphics;
using Foundation;

namespace Xamarin.PropertyEditing.Mac
{
	internal class FocusablePopUpButton : NSPopUpButton
	{
		public FocusablePopUpButton (IHostResourceProvider hostResources)
		{
			if (hostResources == null)
				throw new ArgumentNullException (nameof (hostResources));

			this.hostResources = hostResources;
		}

		public string TextColorName
		{
			get => this.textColorName;
			set
			{
				if (this.textColorName == value)
					return;

				this.textColorName = value;
				AppearanceChanged ();
			}
		}

		public override string Title
		{
			get => base.Title;
			set
			{
				if (base.Title == value)
					return;

				base.Title = value;
				AppearanceChanged ();
			}
		}

		public override bool CanBecomeKeyView { get { return Enabled; } }

		public override bool BecomeFirstResponder ()
		{
			var willBecomeFirstResponder = base.BecomeFirstResponder ();
			if (willBecomeFirstResponder) {
				ScrollRectToVisible (Bounds);
			}
			return willBecomeFirstResponder;
		}

		public override void ViewDidChangeEffectiveAppearance ()
		{
			base.ViewDidChangeEffectiveAppearance ();
			AppearanceChanged ();
		}

		protected virtual void AppearanceChanged ()
		{
			if (this.hostResources == null)
				return;

			var attr = new NSStringAttributes { ForegroundColor = this.hostResources.GetNamedColor (TextColorName) };
			AttributedTitle = new NSAttributedString (Title, attr);

			NSMenuItem[] items = Menu?.Items;
			if (items != null) {
				foreach (NSMenuItem item in items) {
					if (item is ThemedMenuItem themedItem)
						themedItem.AppearanceChanged ();
				}
			}
		}

		private string textColorName = NamedResources.ForegroundColor;
		private readonly IHostResourceProvider hostResources;
	}
}
