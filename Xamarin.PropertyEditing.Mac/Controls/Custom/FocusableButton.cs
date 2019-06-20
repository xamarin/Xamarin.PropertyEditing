using System;
using AppKit;
using Foundation;

namespace Xamarin.PropertyEditing.Mac
{
	internal class FocusableButton : NSButton
	{
		public override bool CanBecomeKeyView { get { return Enabled; } }

		public FocusableButton (IHostResourceProvider hostResources)
		{
			if (hostResources == null)
				throw new ArgumentNullException (nameof (hostResources));

			this.hostResources = hostResources;
			AllowsExpansionToolTips = true;
			AllowsMixedState = true;
			Cell.LineBreakMode = NSLineBreakMode.TruncatingTail;
			Cell.UsesSingleLineMode = true;
			ControlSize = NSControlSize.Small;
			Font = NSFont.SystemFontOfSize (NSFont.SystemFontSizeForControlSize (NSControlSize.Small));
			Title = string.Empty;
			TranslatesAutoresizingMaskIntoConstraints = false;
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
				base.Title = value;
				AppearanceChanged ();
			}
		}

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
		}

		private string textColorName = NamedResources.ForegroundColor;
		private readonly IHostResourceProvider hostResources;
	}
}
